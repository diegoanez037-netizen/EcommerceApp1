using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApp.Data;
using EcommerceApp.Models;

namespace EcommerceApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminOrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminOrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /AdminOrders
        // GET: /AdminOrders
        public async Task<IActionResult> Index(
            string? search,
            OrderStatus? status,
            DateTime? desde,
            DateTime? hasta)
        {
            var query = _context.Orders
                .AsNoTracking()
                .Include(o => o.User)
                .AsQueryable();

            // Buscar por número de pedido, nombre o correo
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                var pattern = $"%{term}%";

                query = query.Where(o =>
                    EF.Functions.ILike(o.OrderNumber, pattern) ||
                    (o.User != null &&
                     (EF.Functions.ILike(o.User.FullName, pattern) ||
                      EF.Functions.ILike(o.User.Email, pattern)))
                );
            }

            // Filtrar por estado
            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            // Filtrar por fecha, usando UTC para PostgreSQL
            if (desde.HasValue)
            {
                var desdeUtc = DateTime.SpecifyKind(
                    desde.Value.Date,
                    DateTimeKind.Utc
                );

                query = query.Where(o => o.CreatedAt >= desdeUtc);
            }

            if (hasta.HasValue)
            {
                var hastaUtc = DateTime.SpecifyKind(
                    hasta.Value.Date.AddDays(1),
                    DateTimeKind.Utc
                );

                query = query.Where(o => o.CreatedAt < hastaUtc);
            }

            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.Desde = desde;
            ViewBag.Hasta = hasta;

            return View(orders);
        }

        // GET: /AdminOrders/Details/1
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.User)
                .Include(o => o.Details)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: /AdminOrders/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(
            int id,
            OrderStatus status)
        {
            if (!Enum.IsDefined(typeof(OrderStatus), status))
            {
                return BadRequest("Estado inválido.");
            }

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            order.Status = status;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "El estado del pedido fue actualizado.";

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}