using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WINGS.BLL.Services;
using WINGS.DAL.Entities;
using WINGS.Web.Filters;
using WINGS.Web.ViewModels;

namespace WINGS.Web.Controllers
{
    
    public class ProductController : Controller
    {
        private readonly ProductService _productService;
        private readonly CategoryService _categoryService;
        private readonly IWebHostEnvironment _environment;
        private readonly CartService _cartService;

        public ProductController(
            ProductService productService,
            CategoryService categoryService,
            IWebHostEnvironment environment,
            CartService cartService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _environment = environment;
            _cartService = cartService;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _productService.GetAllProductsAsync());
        }

        [AdminAuthorize]
        public async Task<IActionResult> Create()
        {
            ProductViewModel vm = new();

            vm.Categories = (await _categoryService.GetAllCategoriesAsync())
                .Select(x => new SelectListItem
                {
                    Text = x.CategoryName,
                    Value = x.CategoryId.ToString()
                }).ToList();

            return View(vm);
        }


        [AdminAuthorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = (await _categoryService.GetAllCategoriesAsync())
                    .Select(x => new SelectListItem
                    {
                        Text = x.CategoryName,
                        Value = x.CategoryId.ToString()
                    }).ToList();

                return View(model);
            }

            string? fileName = null;

            if (model.ImageFile != null)
            {
                string uploadFolder = Path.Combine(_environment.WebRootPath, "uploads", "products");

                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);

                string filePath = Path.Combine(uploadFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }
            }

            Product product = new Product
            {
                ProductName = model.ProductName,
                Description = model.Description,
                Price = model.Price,
                Stock = model.Stock,
                Brand = model.Brand,
                CategoryId = model.CategoryId,
                ImageUrl = fileName == null ? null : "/uploads/products/" + fileName,

                IsFeatured = model.IsFeatured,
                IsTrending = model.IsTrending,
                IsActive = model.IsActive
            };

            await _productService.AddProductAsync(product);

            return RedirectToAction(nameof(Index));
        }

        //View details...

        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);

            if (product == null)
                return NotFound();

            return View(product);
        }

        //Edit product.
        [AdminAuthorize]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);

            if (product == null)
                return NotFound();

            ProductViewModel vm = new()
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                Brand = product.Brand,
                CategoryId = product.CategoryId,
                ImageUrl = product.ImageUrl,

                IsFeatured = product.IsFeatured,
                IsTrending = product.IsTrending,
                IsActive = product.IsActive,

                Categories = (await _categoryService.GetAllCategoriesAsync())
                .Select(x => new SelectListItem
                {
                    Text = x.CategoryName,
                    Value = x.CategoryId.ToString()
                }).ToList()
            };

            return View(vm);
        }

        [AdminAuthorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = (await _categoryService.GetAllCategoriesAsync())
                    .Select(x => new SelectListItem
                    {
                        Text = x.CategoryName,
                        Value = x.CategoryId.ToString()
                    }).ToList();

                return View(model);
            }

            var product = await _productService.GetProductByIdAsync(model.ProductId);

            if (product == null)
                return NotFound();

            product.ProductName = model.ProductName;
            product.Description = model.Description;
            product.Price = model.Price;
            product.Stock = model.Stock;
            product.Brand = model.Brand;
            product.CategoryId = model.CategoryId;
            product.IsFeatured = model.IsFeatured;
            product.IsTrending = model.IsTrending;
            product.IsActive = model.IsActive;

            if (model.ImageFile != null)
            {
                string folder = Path.Combine(_environment.WebRootPath,
                                             "uploads",
                                             "products");

                string fileName = Guid.NewGuid().ToString()
                                + Path.GetExtension(model.ImageFile.FileName);

                using (var stream = new FileStream(Path.Combine(folder, fileName),
                                                  FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }

                product.ImageUrl = "/uploads/products/" + fileName;
            }

            await _productService.UpdateProductAsync(product);

            return RedirectToAction(nameof(Index));
        }

        // DELETE
        [AdminAuthorize]
        public async Task<IActionResult> Delete(int id)
        {
            await _productService.DeleteProductAsync(id);
            return RedirectToAction(nameof(Index));
        }

        //---------------------------------------------------Add to card function--------------------------------------//
        //
        [HttpGet]
        public async Task<IActionResult> AddToCart(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                TempData["Error"] = "Please login first to add products to your cart.";
                return RedirectToAction("Login", "Account");
            }

            Cart cart = new Cart
            {
                ProductId = id,
                UserId = userId.Value,
                Quantity = 1,
                AddedDate = DateTime.Now
            };

            await _cartService.AddToCartAsync(cart);

            TempData["Success"] = "Product added to cart successfully.";

            return RedirectToAction("Cart");
        }
        public async Task<IActionResult> Cart()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                TempData["Error"] = "Please login first.";
                return RedirectToAction("Login", "Account");
            }

            var cartItems = await _cartService.GetCartByUserAsync(userId.Value);

            return View(cartItems);
        }
        public async Task<IActionResult> RemoveCart(int id)
        {
            await _cartService.RemoveCartAsync(id);

            return RedirectToAction(nameof(Cart));
        }

        //--------------------------------------------Quantity increase and decrease functions---------------------------------------

        public async Task<IActionResult> IncreaseQuantity(int id)
        {
            await _cartService.IncreaseQuantityAsync(id);

            return RedirectToAction(nameof(Cart));
        }
        public async Task<IActionResult> DecreaseQuantity(int id)
        {
            await _cartService.DecreaseQuantityAsync(id);

            return RedirectToAction(nameof(Cart));
        }
    }
}