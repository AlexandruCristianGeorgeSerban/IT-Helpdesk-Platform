using IT_Helpdesk_Platform.Models;
using IT_Helpdesk_Platform.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;

namespace IT_Helpdesk_Platform.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly IUserService _userService;
        private readonly IDepartmentService _departmentService;

        public UsersController(IUserService userService, IDepartmentService departmentService)
        {
            _userService = userService;
            _departmentService = departmentService;
        }

        public async Task<IActionResult> Index(string? searchString)
        {
            ViewData["CurrentFilter"] = searchString;
            var users = await _userService.GetFilteredUsersAsync(searchString);
            return View(users);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var user = await _userService.GetUserByIdWithDepartmentAsync(id.Value);
            return user == null ? NotFound() : View(user);
        }

        public async Task<IActionResult> Create()
        {
            ViewData["DepartmentId"] = new SelectList(_departmentService.GetAllDepartments(), "Id", "Name");
            ViewData["Roles"] = new SelectList(await _userService.GetAllRolesAsync());
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FirstName,LastName,Email,PhoneNumber,DepartmentId,Role")] User formUser, string? PasswordHash)
        {
            ModelState.Clear();
            TryValidateModel(formUser);

            if (ModelState.IsValid)
            {
                string finalPassword = string.IsNullOrEmpty(PasswordHash) ? "Helpdesk123!" : PasswordHash;

                var result = await _userService.CreateUserWithRoleAsync(formUser, finalPassword, formUser.Role ?? "User");

                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "User created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
            }

            ViewData["DepartmentId"] = new SelectList(_departmentService.GetAllDepartments(), "Id", "Name", formUser.DepartmentId);
            ViewData["Roles"] = new SelectList(await _userService.GetAllRolesAsync(), formUser.Role);
            return View(formUser);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var user = await _userService.GetUserByIdWithDepartmentAsync(id.Value);
            if (user == null) return NotFound();

            await PrepareEditViewBag(user);
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,Email,PhoneNumber,DepartmentId,Role")] User formUser, string? PasswordHash)
        {
            if (id != formUser.Id) return NotFound();

            ModelState.Clear();
            TryValidateModel(formUser);

            if (ModelState.IsValid)
            {
                var result = await _userService.UpdateUserWithRoleAsync(formUser, formUser.Role ?? "User", PasswordHash);

                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "User updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
            }

            await PrepareEditViewBag(formUser);
            return View(formUser);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var user = await _userService.GetUserByIdWithDepartmentAsync(id.Value);
            return user == null ? NotFound() : View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var result = await _userService.DeleteUserAsync(id);
                if (result.Succeeded)
                    TempData["SuccessMessage"] = "User and all related data deleted successfully.";
                else
                    TempData["ErrorMessage"] = "Could not delete user.";
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = "Database constraint error: " + ex.InnerException?.Message ?? ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task PrepareEditViewBag(User user)
        {
            ViewData["DepartmentId"] = new SelectList(_departmentService.GetAllDepartments(), "Id", "Name", user.DepartmentId);
            ViewData["Roles"] = new SelectList(await _userService.GetAllRolesAsync(), user.Role);
        }
    }
}