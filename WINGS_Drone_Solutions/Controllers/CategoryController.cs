using Microsoft.AspNetCore.Mvc;
using WINGS.BLL.Services;
using WINGS.DAL.Entities;
using WINGS.Web.Filters;
using WINGS.Web.ViewModels;

namespace WINGS.Web.Controllers
{
    [AdminAuthorize]
    public class CategoryController : Controller
    {
        private readonly CategoryService _categoryService;
        private readonly IWebHostEnvironment _environment;

        public CategoryController(CategoryService categoryService,
                                  IWebHostEnvironment environment)
        {
            _categoryService = categoryService;
            _environment = environment;
        }

        // GET: Category
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return View(categories);
        }

        // GET: Category/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string? fileName = null;

            if (model.ImageFile != null)
            {
                string uploadFolder = Path.Combine(_environment.WebRootPath, "uploads", "categories");

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

            Category category = new Category
            {
                CategoryName = model.CategoryName,
                Description = model.Description,
                ImageUrl = fileName == null ? null : "/uploads/categories/" + fileName
            };

            await _categoryService.AddCategoryAsync(category);

            return RedirectToAction(nameof(Index));
        }

        // GET: Category/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);

            if (category == null)
                return NotFound();

            return View(category);
        }

        // POST: Category/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                await _categoryService.UpdateCategoryAsync(category);
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        // GET: Category/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);

            if (category == null)
                return NotFound();

            return View(category);
        }

        // POST: Category/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _categoryService.DeleteCategoryAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}