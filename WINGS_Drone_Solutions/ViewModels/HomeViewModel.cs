using WINGS.DAL.Entities;

namespace WINGS.Web.ViewModels
{
    public class HomeViewModel
    {
        public List<Category> Categories { get; set; } = new();

        public List<Product> FeaturedProducts { get; set; } = new();

        public List<Product> TrendingProducts { get; set; } = new();

        public List<Product> LatestProducts { get; set; } = new();
    }
}