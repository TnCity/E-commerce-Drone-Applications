using System.ComponentModel.DataAnnotations;
using WINGS.DAL.Entities;

namespace WINGS.Web.ViewModels
{
    public class CheckoutViewModel
    {
        [Required]
        [Display(Name = "Shipping Address")]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; } = string.Empty;

        public List<Cart> CartItems { get; set; } = new();

        public decimal GrandTotal { get; set; }
    }
}