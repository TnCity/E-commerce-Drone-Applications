using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace WINGS.Web.ViewModels
{
    public class ProductViewModel
    {
        public int ProductId { get; set; }

        [Required]
        public string ProductName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public int Stock { get; set; }

        public string? Brand { get; set; }

        public int CategoryId { get; set; }

        public string? ImageUrl { get; set; }

        public IFormFile? ImageFile { get; set; }

        public List<SelectListItem>? Categories { get; set; }
        public bool IsFeatured { get; set; }

        public bool IsTrending { get; set; }

        public bool IsActive { get; set; } = true;
    }
}