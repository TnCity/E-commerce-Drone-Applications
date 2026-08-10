using System.ComponentModel.DataAnnotations;

namespace WINGS.Web.ViewModels
{
    public class PaymentViewModel
    {
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Please select a payment method.")]
        public string PaymentMethod { get; set; } = "UPI";

        public string? UPIId { get; set; }

        public string? CardNumber { get; set; }

        public string? ExpiryDate { get; set; }

        public string? CVV { get; set; }
    }
}