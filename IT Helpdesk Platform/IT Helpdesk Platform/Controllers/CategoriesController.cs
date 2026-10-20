using IT_Helpdesk_Platform.Models;
using IT_Helpdesk_Platform.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IT_Helpdesk_Platform.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public IActionResult Index(string? searchString)
        {
            ViewData["CurrentFilter"] = searchString;
            var categories = _categoryService.GetAllCategories(searchString);
            return View(categories);
        }

        public IActionResult Details(int? id)
        {
            if (id == null) return NotFound();

            var category = _categoryService.GetCategoryById(id.Value);
            if (category == null) return NotFound();

            return View(category);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Id,Name")] Category category)
        {
            if (ModelState.IsValid)
            {
                _categoryService.CreateCategory(category);
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null) return NotFound();

            var category = _categoryService.GetCategoryById(id.Value);
            if (category == null) return NotFound();

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("Id,Name")] Category category)
        {
            if (id != category.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _categoryService.UpdateCategory(category);
                }
                catch
                {
                    if (!_categoryService.CategoryExists(category.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null) return NotFound();

            var category = _categoryService.GetCategoryById(id.Value);
            if (category == null) return NotFound();

            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _categoryService.DeleteCategory(id);
            return RedirectToAction(nameof(Index));
        }
    }
}