using Microsoft.AspNetCore.Mvc;
using WINGS.BLL.Services;
using WINGS.DAL.Entities;
using WINGS.Web.ViewModels;

namespace WINGS.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ProductService _productService;
        private readonly CategoryService _categoryService;
        private readonly ContactMessageService _contactMessageService;

        public HomeController(ProductService productService,
                              CategoryService categoryService,
                              ContactMessageService contactMessageService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _contactMessageService = contactMessageService;
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
        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(
            ContactViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var contactMessage = new ContactMessage
            {
                FullName = model.FullName,
                Email = model.Email,
                Phone = model.Phone,
                Subject = model.Subject,
                Message = model.Message,
                CreatedAt = DateTime.Now,
                IsRead = false
            };

            await _contactMessageService
                .AddMessageAsync(contactMessage);

            TempData["Success"] =
                "Thank you! Your message has been sent successfully.";

            return RedirectToAction(nameof(Contact));
        }
    }
}