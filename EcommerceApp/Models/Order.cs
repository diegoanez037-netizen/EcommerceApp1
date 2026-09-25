using System.ComponentModel.DataAnnotations;

namespace EcommerceApp.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(40)]
        public string OrderNumber { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; } = string.Empty;

        public ApplicationUser? User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pendiente;

        public decimal Total { get; set; }

        [MaxLength(300)]
        public string? ShippingAddress { get; set; }

        public ICollection<OrderDetail> Details { get; set; }
            = new List<OrderDetail>();
    }
}