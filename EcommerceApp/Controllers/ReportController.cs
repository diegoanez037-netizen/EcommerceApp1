using EcommerceApp.Data;
using EcommerceApp.Models;
using FastReport;
using FastReport.Export.PdfSimple;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ReportController(
            ApplicationDbContext context,
            IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: /Report/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await CargarCategorias();

            return View();
        }

        // POST: /Report/Catalogo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Catalogo(
            DateTime? desde,
            DateTime? hasta,
            string? categoria,
            DateTime? fechaReporte)
        {
            if (!fechaReporte.HasValue)
            {
                ModelState.AddModelError(
                    "fechaReporte",
                    "Selecciona la fecha y la hora del reporte."
                );

                await CargarCategorias();

                return View(nameof(Index));
            }

            // Por ahora mostrar todos los productos del catálogo
            var productos = await _context.Products
                .OrderBy(p => p.Category)
                .ThenBy(p => p.Name)
                .ToListAsync();

            byte[] pdf = await GenerarPdf(productos, fechaReporte.Value);

            string nombreArchivo =
                $"CatalogoProductos_{fechaReporte.Value:yyyyMMdd_HHmmss}.pdf";

            // El tercer parámetro hace que se descargue
            return File(
                pdf,
                "application/pdf",
                nombreArchivo
            );
        }

        // Cargar las categorías para el formulario
        private async Task CargarCategorias()
        {
            ViewBag.Categorias = await _context.Products
                .Where(p =>
                    p.Category != null &&
                    p.Category != ""
                )
                .Select(p => p.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        // Generar el PDF con FastReport
        private async Task<byte[]> GenerarPdf(
            List<Product> productos,
            DateTime fechaEmision)
        {
            // Crear tabla del reporte
            var tabla = new System.Data.DataTable("Productos");

            tabla.Columns.Add("Numero", typeof(int));
            tabla.Columns.Add("Nombre", typeof(string));
            tabla.Columns.Add("Categoria", typeof(string));
            tabla.Columns.Add("Descripcion", typeof(string));
            tabla.Columns.Add("Fecha", typeof(string));
            tabla.Columns.Add("Precio", typeof(string));
            tabla.Columns.Add("Stock", typeof(int));
            tabla.Columns.Add("ValorStock", typeof(string));

            int numero = 1;

            foreach (var producto in productos)
            {
                tabla.Rows.Add(
                    numero++,
                    producto.Name,
                    producto.Category ?? "Sin categoría",
                    producto.Description,
                    producto.CreatedAt.ToString("dd/MM/yyyy"),
                    $"Bs. {producto.Price:N2}",
                    producto.Stock,
                    $"Bs. {producto.Price * producto.Stock:N2}"
                );
            }

            // Calcular totales
            int totalProductos = productos.Count;

            decimal precioPromedio = productos.Count > 0
                ? productos.Average(p => p.Price)
                : 0m;

            decimal valorInventario = productos.Sum(
                p => p.Price * p.Stock
            );

            // Ubicar plantilla FastReport
            string rutaPlantilla = Path.Combine(
                _env.ContentRootPath,
                "Reports",
                "CatalogoProductos.frx"
            );

            if (!System.IO.File.Exists(rutaPlantilla))
            {
                throw new FileNotFoundException(
                    "No se encontró la plantilla del reporte."
                );
            }

            // Preparar FastReport
            FastReport.Utils.Config.WebMode = true;

            using var reporte = new Report();
            reporte.Load(rutaPlantilla);

            // Registrar datos
            reporte.RegisterData(tabla, "Productos");
            reporte.GetDataSource("Productos").Enabled = true;

            // Parámetros del reporte
            reporte.SetParameterValue(
                "TotalProductos",
                totalProductos
            );

            reporte.SetParameterValue(
                "PrecioPromedio",
                $"Bs. {precioPromedio:N2}"
            );

            reporte.SetParameterValue(
                "ValorInventario",
                $"Bs. {valorInventario:N2}"
            );

            // Fecha y hora elegida por el usuario
            reporte.SetParameterValue(
                "FechaEmision",
                fechaEmision.ToString("dd/MM/yyyy 'a las' HH:mm:ss")
            );

            // Preparar y exportar
            reporte.Prepare();

            using var memoryStream = new MemoryStream();

            var exportPdf = new PDFSimpleExport();
            reporte.Export(exportPdf, memoryStream);

            return memoryStream.ToArray();
        }
    }
}