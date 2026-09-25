using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EcommerceApp.Models;

namespace EcommerceApp.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>(entity =>
            {
                entity.Property(o => o.Total)
                    .HasPrecision(18, 2);

                entity.HasOne(o => o.User)
                    .WithMany()
                    .HasForeignKey(o => o.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(o => o.Details)
                    .WithOne(d => d.Order)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.Property(d => d.UnitPrice)
                    .HasPrecision(18, 2);

                entity.Property(d => d.Subtotal)
                    .HasPrecision(18, 2);

                entity.HasOne(d => d.Product)
                    .WithMany()
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            // Productos de prueba: bebidas alcohólicas
            modelBuilder.Entity<Product>().HasData(

                new Product
                {
                    Id = 1,
                    Name = "Cerveza Paceña",
                    Description = "Cerveza lager boliviana de 620 ml.",
                    Price = 12.00m,
                    Stock = 25,
                    Category = "Cerveza",
                    ImageUrl = null,
                    CreatedAt = new DateTime(2026, 9, 10)
                },

                new Product
                {
                    Id = 2,
                    Name = "Cerveza Huari",
                    Description = "Cerveza boliviana de 620 ml.",
                    Price = 15.00m,
                    Stock = 20,
                    Category = "Cerveza",
                    ImageUrl = null,
                    CreatedAt = new DateTime(2026, 9, 10)
                },

                new Product
                {
                    Id = 3,
                    Name = "Singani Casa Real",
                    Description = "Singani boliviano de 750 ml.",
                    Price = 65.00m,
                    Stock = 15,
                    Category = "Singani",
                    ImageUrl = null,
                    CreatedAt = new DateTime(2026, 9, 10)
                },

                new Product
                {
                    Id = 4,
                    Name = "Ron Abuelo",
                    Description = "Ron añejo de 750 ml.",
                    Price = 95.00m,
                    Stock = 10,
                    Category = "Ron",
                    ImageUrl = null,
                    CreatedAt = new DateTime(2026, 9, 10)
                },

                new Product
                {
                    Id = 5,
                    Name = "Johnnie Walker Red Label",
                    Description = "Whisky escocés de 750 ml.",
                    Price = 180.00m,
                    Stock = 8,
                    Category = "Whisky",
                    ImageUrl = null,
                    CreatedAt = new DateTime(2026, 9, 10)
                },

                new Product
                {
                    Id = 6,
                    Name = "Vodka Smirnoff",
                    Description = "Vodka de 750 ml.",
                    Price = 110.00m,
                    Stock = 12,
                    Category = "Vodka",
                    ImageUrl = null,
                    CreatedAt = new DateTime(2026, 9, 10)
                },

                new Product
                {
                    Id = 7,
                    Name = "Fernet Branca",
                    Description = "Bebida alcohólica italiana de 750 ml.",
                    Price = 120.00m,
                    Stock = 10,
                    Category = "Fernet",
                    ImageUrl = null,
                    CreatedAt = new DateTime(2026, 9, 10)
                },

                new Product
                {
                    Id = 8,
                    Name = "Tequila José Cuervo",
                    Description = "Tequila de 750 ml.",
                    Price = 150.00m,
                    Stock = 7,
                    Category = "Tequila",
                    ImageUrl = null,
                    CreatedAt = new DateTime(2026, 9, 10)
                },

                new Product
                {
                    Id = 9,
                    Name = "Vino Tinto",
                    Description = "Vino tinto de 750 ml.",
                    Price = 55.00m,
                    Stock = 18,
                    Category = "Vino",
                    ImageUrl = null,
                    CreatedAt = new DateTime(2026, 9, 10)
                },

                new Product
                {
                    Id = 10,
                    Name = "Cerveza Corona",
                    Description = "Cerveza lager de 355 ml.",
                    Price = 18.00m,
                    Stock = 20,
                    Category = "Cerveza",
                    ImageUrl = null,
                    CreatedAt = new DateTime(2026, 9, 10)
                }
            );
        }
    }
}