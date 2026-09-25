using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApp.Data;
using EcommerceApp.Models;

namespace EcommerceApp.Controllers
{
    [Authorize]
    public class CartController(ApplicationDbContext context, UserManager<ApplicationUser> userManager) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var userId = userManager.GetUserId(User);

            var items = await context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int productId, int quantity = 1)
        {
            var userId = userManager.GetUserId(User);

            var existing = await context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);

            if (existing != null)
            {
                existing.Quantity += quantity;
            }
            else
            {
                context.CartItems.Add(new CartItem
                {
                    UserId = userId!,
                    ProductId = productId,
                    Quantity = quantity
                });
            }

            await context.SaveChangesAsync();
            return RedirectToAction("Index", "Products");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int id, int quantity)
        {
            var userId = userManager.GetUserId(User);
            var item = await context.CartItems.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (item != null)
            {
                if (quantity <= 0)
                    context.CartItems.Remove(item);
                else
                    item.Quantity = quantity;

                await context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int id)
        {
            var userId = userManager.GetUserId(User);
            var item = await context.CartItems.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (item != null)
            {
                context.CartItems.Remove(item);
                await context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout()
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return Challenge();
            }

            var items = await context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (items.Count == 0)
            {
                TempData["CheckoutError"] =
                    "Tu carrito está vacío.";

                return RedirectToAction(nameof(Index));
            }

            foreach (var item in items)
            {
                if (item.Product == null)
                {
                    TempData["CheckoutError"] =
                        "Uno de los productos ya no está disponible.";

                    return RedirectToAction(nameof(Index));
                }

                if (item.Quantity <= 0 ||
                    item.Quantity > item.Product.Stock)
                {
                    TempData["CheckoutError"] =
                        $"No hay suficiente stock de {item.Product.Name}.";

                    return RedirectToAction(nameof(Index));
                }
            }

            await using var transaction =
                await context.Database.BeginTransactionAsync();

            try
            {
                decimal total = 0m;

                var order = new Order
                {
                    OrderNumber =
                        $"CA-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}",

                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    Status = OrderStatus.Pendiente,
                    ShippingAddress = user.Address,
                    Details = new List<OrderDetail>()
                };

                foreach (var item in items)
                {
                    var product = item.Product!;

                    decimal subtotal =
                        product.Price * item.Quantity;

                    order.Details.Add(new OrderDetail
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        UnitPrice = product.Price,
                        Quantity = item.Quantity,
                        Subtotal = subtotal
                    });

                    product.Stock -= item.Quantity;
                    total += subtotal;
                }

                order.Total = total;

                context.Orders.Add(order);
                context.CartItems.RemoveRange(items);

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["CheckoutSuccess"] = "true";
                TempData["OrderNumber"] = order.OrderNumber;

                return RedirectToAction(
    "Details",
    "Orders",
    new { id = order.Id }
);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
