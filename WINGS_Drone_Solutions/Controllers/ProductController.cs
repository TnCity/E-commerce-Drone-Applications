using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WINGS.BLL.Services;
using WINGS.DAL.Entities;
using WINGS.Web.Filters;
using WINGS.Web.ViewModels;

namespace WINGS.Web.Controllers
{
    [AdminAuthorize]
    public class ProductController : Controller
    {
        private readonly ProductService _productService;
        private readonly CategoryService _categoryService;
        private readonly IWebHostEnvironment _environment;

        public ProductController(
            ProductService productService,
            CategoryService categoryService,
            IWebHostEnvironment environment)
        {
            _productService = productService;
            _categoryService = categoryService;
            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _productService.GetAllProductsAsync());
        }

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

        public async Task<IActionResult> Delete(int id)
        {
            await _productService.DeleteProductAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}