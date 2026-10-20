using IT_Helpdesk_Platform.Models;
using IT_Helpdesk_Platform.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IT_Helpdesk_Platform.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DepartmentsController : Controller
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentsController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        public IActionResult Index(string? searchString)
        {
            ViewData["CurrentFilter"] = searchString;
            var departments = _departmentService.GetAllDepartments(searchString);
            return View(departments);
        }

        public IActionResult Details(int? id)
        {
            if (id == null) return NotFound();

            var department = _departmentService.GetDepartmentById(id.Value);
            if (department == null) return NotFound();

            return View(department);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Id,Name")] Department department)
        {
            if (ModelState.IsValid)
            {
                _departmentService.CreateDepartment(department);
                return RedirectToAction(nameof(Index));
            }
            return View(department);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null) return NotFound();

            var department = _departmentService.GetDepartmentById(id.Value);
            if (department == null) return NotFound();

            return View(department);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("Id,Name")] Department department)
        {
            if (id != department.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _departmentService.UpdateDepartment(department);
                }
                catch
                {
                    if (!_departmentService.DepartmentExists(department.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(department);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null) return NotFound();

            var department = _departmentService.GetDepartmentById(id.Value);
            if (department == null) return NotFound();

            return View(department);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _departmentService.DeleteDepartment(id);
            return RedirectToAction(nameof(Index));
        }
    }
}