using IT_Helpdesk_Platform.Models;
using IT_Helpdesk_Platform.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IT_Helpdesk_Platform.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly IDepartmentService _departmentService;

        public AccountController(IUserService userService, IDepartmentService departmentService)
        {
            _userService = userService;
            _departmentService = departmentService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            ViewBag.Departments = new SelectList(_departmentService.GetAllDepartments(), "Id", "Name");
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _userService.Authenticate(model.Email!, model.Password!);

                if (user != null)
                {
                    if (user.Role == "Admin") return RedirectToAction("AdministratorDashboard", "Home");
                    if (user.Role == "Agent") return RedirectToAction("AgentDashboard", "Home");
                    return RedirectToAction("UserDashboard", "Home");
                }
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            ViewBag.Departments = new SelectList(_departmentService.GetAllDepartments(), "Id", "Name");
            return View(model);
        }

        public IActionResult Profile()
        {
            // Temporary Logic (Placeholder: User ID = 1)
            var user = _userService.GetUserById(1);

            if (user == null)
            {
                return NotFound("The user with ID = 1 wasn't found in the database.");
            }

            return View(user);
        }

        [HttpPost]
        public IActionResult UpdateProfile(int Id, string FirstName, string LastName, string PhoneNumber, bool EmailNotificationsEnabled, bool PushNotificationsEnabled)
        {
            var user = _userService.GetUserById(Id);

            if (user != null)
            {
                user.FirstName = FirstName;
                user.LastName = LastName;
                user.PhoneNumber = PhoneNumber;

                user.EmailNotificationsEnabled = EmailNotificationsEnabled;
                user.PushNotificationsEnabled = PushNotificationsEnabled;

                _userService.UpdateUser(user);

                TempData["ProfileSuccess"] = "Profile successfully updated!";
            }
            return RedirectToAction("Profile");
        }

        [HttpPost]
        public IActionResult RegisterAjax(string FirstName, string LastName, int DepartmentId, string Email, string Password, string ConfirmPassword)
        {
            var result = _userService.RegisterNewUser(FirstName, LastName, DepartmentId, Email, Password, ConfirmPassword);

            if (!result.Success)
            {
                return Json(new { success = false, message = result.Message });
            }

            return Json(new { success = true, redirectUrl = "/Home/UserDashboard" });
        }
    }
}