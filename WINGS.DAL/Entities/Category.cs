using System.ComponentModel.DataAnnotations;

namespace WINGS.DAL.Entities
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        public string CategoryName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }
    }
}