using Microsoft.AspNetCore.Mvc;
using WINGS.BLL.Services;
using WINGS.Web.ViewModels;

namespace WINGS.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ProductService _productService;
        private readonly CategoryService _categoryService;

        public HomeController(ProductService productService,
                              CategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            var products = await _productService.GetAllProductsAsync();

            HomeViewModel model = new HomeViewModel
            {
                Categories = categories.ToList(),

                FeaturedProducts = products
                    .Where(x => x.IsFeatured && x.IsActive)
                    .Take(8)
                    .ToList(),

                TrendingProducts = products
                    .Where(x => x.IsTrending && x.IsActive)
                    .Take(8)
                    .ToList(),

                LatestProducts = products
                    .Where(x => x.IsActive)
                    .OrderByDescending(x => x.ProductId)
                    .Take(8)
                    .ToList()
            };

            return View(model);
        }

        //Contact page
        public IActionResult Contact()
        {
            return View();
        }
    }
}