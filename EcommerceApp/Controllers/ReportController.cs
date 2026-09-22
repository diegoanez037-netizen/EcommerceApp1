using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApp.Data;
using FastReport;
using FastReport.Export.PdfSimple;

namespace EcommerceApp.Controllers
{
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ReportController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: /Report
        public IActionResult Index() => RedirectToAction(nameof(Catalogo));

        // GET: /Report/Catalogo
        public async Task<IActionResult> Catalogo()
        {
            // 1. Obtener los productos de la base de datos
            var productos = await _context.Products
                .OrderBy(p => p.Category)
                .ThenBy(p => p.Name)
                .ToListAsync();

            // 2. Crear DataTable con los datos de detalle
            var tabla = new System.Data.DataTable("Productos");
            tabla.Columns.Add("Numero",      typeof(int));
            tabla.Columns.Add("Nombre",      typeof(string));
            tabla.Columns.Add("Categoria",   typeof(string));
            tabla.Columns.Add("Descripcion", typeof(string));
            tabla.Columns.Add("Fecha",       typeof(string));   // Fecha de registro (dd/MM/yyyy)
            tabla.Columns.Add("Precio",      typeof(string));   // pre-formateado con "Bs."
            tabla.Columns.Add("Stock",       typeof(int));
            tabla.Columns.Add("ValorStock",  typeof(string));   // pre-formateado con "Bs."

            int num = 1;
            foreach (var p in productos)
            {
                tabla.Rows.Add(
                    num++,
                    p.Name,
                    p.Category ?? "Sin categoría",
                    p.Description,
                    p.CreatedAt.ToString("dd/MM/yyyy"),
                    $"Bs. {p.Price:N2}",
                    p.Stock,
                    $"Bs. {p.Price * p.Stock:N2}"
                );
            }

            // 3. Calcular totales / agregaciones en C#
            int    totalProductos  = productos.Count;
            decimal precioPromedio = productos.Count > 0
                ? productos.Average(p => p.Price)
                : 0m;
            decimal valorInventario = productos.Sum(p => p.Price * p.Stock);

            // 4. Cargar la plantilla del reporte
            string rutaPlantilla = Path.Combine(_env.ContentRootPath, "Reports", "CatalogoProductos.frx");

            FastReport.Utils.Config.WebMode = true;
            using var reporte = new Report();
            reporte.Load(rutaPlantilla);

            // 5. Registrar el DataTable
            reporte.RegisterData(tabla, "Productos");
            reporte.GetDataSource("Productos").Enabled = true;

            // 6. Pasar los totales como parámetros
            reporte.SetParameterValue("TotalProductos",  totalProductos);
            reporte.SetParameterValue("PrecioPromedio",  $"Bs. {precioPromedio:N2}");
            reporte.SetParameterValue("ValorInventario", $"Bs. {valorInventario:N2}");
            reporte.SetParameterValue("FechaEmision",    DateTime.Now.ToString("dd/MM/yyyy"));

            // 7. Preparar y exportar a PDF
            reporte.Prepare();

            using var ms = new MemoryStream();
            var exportPdf = new PDFSimpleExport();
            reporte.Export(exportPdf, ms);
            ms.Position = 0;

            return File(ms.ToArray(), "application/pdf", "CatalogoProductos.pdf");
        }
    }
}
