using System.ComponentModel.DataAnnotations;

namespace EcommerceApp.Models
{
    public class OrderDetail
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        public Order? Order { get; set; }

        [Required]
        public int ProductId { get; set; }

        public Product? Product { get; set; }

        [Required, MaxLength(100)]
        public string ProductName { get; set; } = string.Empty;

        public decimal UnitPrice { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        public decimal Subtotal { get; set; }
    }
}