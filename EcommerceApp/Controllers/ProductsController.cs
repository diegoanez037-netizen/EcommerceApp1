using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApp.Data;
using EcommerceApp.Models;

namespace EcommerceApp.Controllers
{
    // Constructor primario: "context" reemplaza el campo _context de antes.
    [Authorize]
    public class ProductsController(ApplicationDbContext context) : Controller
    {
        [AllowAnonymous]
        public async Task<IActionResult> Index(string? search, string? category)
        {
            var query = context.Products.AsNoTracking().AsQueryable();

            // Limpia palabras provenientes de voz, por ejemplo: "Cerveza."
            var normalizedSearch = search?
                .Trim()
                .TrimEnd('.', ',', ';', ':', '!', '?');

            if (!string.IsNullOrWhiteSpace(normalizedSearch))
            {
                var pattern = $"%{normalizedSearch}%";

                query = query.Where(p =>
                    EF.Functions.ILike(p.Name, pattern) ||
                    EF.Functions.ILike(p.Description, pattern));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(p => p.Category == category);
            }

            var products = await query.ToListAsync();

            ViewBag.Categories = await context.Products
                .Where(p => p.Category != null && p.Category != "")
                .Select(p => p.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            ViewBag.TotalCategories = ViewBag.Categories.Count;
            ViewBag.CurrentCategory = category;
            ViewBag.CurrentSearch = normalizedSearch;

            return View(products);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var product = await context.Products.FindAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Product product)
        {
            if (!ModelState.IsValid) return View(product);
            context.Products.Add(product);
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await context.Products.FindAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            if (id != product.Id) return NotFound();
            if (!ModelState.IsValid) return View(product);

            product.UpdatedAt = DateTime.UtcNow;
            context.Products.Update(product);
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await context.Products.FindAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await context.Products.FindAsync(id);
            if (product != null)
            {
                context.Products.Remove(product);
                await context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}