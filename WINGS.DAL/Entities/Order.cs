using System.ComponentModel.DataAnnotations;

namespace WINGS.DAL.Entities
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        public int UserId { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        public decimal TotalAmount { get; set; }

        [Required]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required]
        public string Phone { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending";

        // Navigation
        public User? User { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; }
            = new List<OrderItem>();
    }
}