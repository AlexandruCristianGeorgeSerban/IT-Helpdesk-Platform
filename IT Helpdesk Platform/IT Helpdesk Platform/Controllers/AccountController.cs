using IT_Helpdesk_Platform.Models;
using IT_Helpdesk_Platform.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IT_Helpdesk_Platform.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IDepartmentService _departmentService;
        private readonly IAuthService _authService;

        public AccountController(
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            IDepartmentService departmentService,
            IAuthService authService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _departmentService = departmentService;
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            ViewBag.Departments = new SelectList(_departmentService.GetAllDepartments(), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email!, model.Password!, model.RememberMe, lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email!);
                    if (user?.Role == "Admin") return RedirectToAction("AdministratorDashboard", "Home");
                    if (user?.Role == "Agent") return RedirectToAction("AgentDashboard", "Home");
                    return RedirectToAction("UserDashboard", "Home");
                }

                if (result.IsLockedOut)
                {
                    int? minutesLeft = await _authService.GetLockoutMinutesLeftAsync(model.Email!);
                    string timeMessage = minutesLeft.HasValue ? $"Please wait {minutesLeft} minute(s)." : "Please wait a few minutes.";

                    ModelState.AddModelError(string.Empty, "Your account has been temporarily locked due to too many failed attempts.");
                    ModelState.AddModelError(string.Empty, timeMessage);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    ModelState.AddModelError(string.Empty, "Please check your credentials.");

                    int? attemptsLeft = await _authService.GetAttemptsLeftAsync(model.Email!);
                    if (attemptsLeft > 0)
                    {
                        ModelState.AddModelError(string.Empty, $"You have {attemptsLeft} attempt(s) remaining before your account is locked.");
                    }
                }
            }

            ViewBag.Departments = new SelectList(_departmentService.GetAllDepartments(), "Id", "Name");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("GuestDashboard", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            if (User.Identity?.Name == null) return RedirectToAction("Login");

            var user = await _authService.GetUserProfileWithDetailsAsync(User.Identity.Name);
            if (user == null)
            {
                await _signInManager.SignOutAsync();
                return RedirectToAction("Login");
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(int Id, string FirstName, string LastName, string PhoneNumber, bool EmailNotificationsEnabled, bool PushNotificationsEnabled, IFormFile? ProfilePicture)
        {
            bool success = await _authService.UpdateProfileAsync(Id, FirstName, LastName, PhoneNumber, EmailNotificationsEnabled, PushNotificationsEnabled, ProfilePicture);

            if (success) TempData["ProfileSuccess"] = "Profile successfully updated!";
            return RedirectToAction("Profile");
        }

        [HttpPost]
        public async Task<IActionResult> RegisterAjax(string FirstName, string LastName, int DepartmentId, string Email, string Password, string ConfirmPassword)
        {
            try
            {
                if (Password != ConfirmPassword)
                    return Json(new { success = false, message = "Passwords do not match." });

                var (success, errors, user) = await _authService.RegisterUserAsync(FirstName, LastName, DepartmentId, Email, Password);

                if (!success)
                    return Json(new { success = false, message = string.Join("<br>", errors) });

                await _signInManager.SignInAsync(user!, isPersistent: false);
                return Json(new { success = true, email = Email, redirectUrl = "/Home/UserDashboard" });
            }
            catch (System.Exception ex)
            {
                string errorDetails = ex.Message;
                if (ex.InnerException != null) errorDetails += " | " + ex.InnerException.Message;

                return Json(new { success = false, message = $"Eroare Server: {errorDetails}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> VerifyAccountManual([FromForm] string email)
        {
            if (string.IsNullOrEmpty(email)) return Json(new { success = false, message = "Email is missing!" });

            bool success = await _authService.VerifyAccountAsync(email);
            if (!success) return Json(new { success = false, message = "User not found or verification failed." });

            return Json(new { success = true });
        }
    }
}