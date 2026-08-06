using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace WINGS.Web.ViewModels
{
    public class CategoryViewModel
    {
        public int CategoryId { get; set; }

        [Required]
        public string CategoryName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public IFormFile? ImageFile { get; set; }
    }
}