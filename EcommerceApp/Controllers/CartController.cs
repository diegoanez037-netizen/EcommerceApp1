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
            var items = await context.CartItems.Where(c => c.UserId == userId).ToListAsync();

            context.CartItems.RemoveRange(items);
            await context.SaveChangesAsync();

            TempData["CheckoutSuccess"] = "true";
            return RedirectToAction(nameof(Index));
        }
    }
}