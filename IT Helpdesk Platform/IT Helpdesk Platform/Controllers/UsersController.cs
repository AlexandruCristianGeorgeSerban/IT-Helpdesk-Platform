using IT_Helpdesk_Platform.Models;
using IT_Helpdesk_Platform.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IT_Helpdesk_Platform.Controllers
{
    public class UsersController : Controller
    {
        private readonly IUserService _userService;
        private readonly IDepartmentService _departmentService;

        public UsersController(IUserService userService, IDepartmentService departmentService)
        {
            _userService = userService;
            _departmentService = departmentService;
        }

        // GET: Users
        public IActionResult Index(string? searchString)
        {
            ViewData["CurrentFilter"] = searchString;
            var users = _userService.GetAllUsers(searchString);
            return View(users);
        }

        // GET: Users/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null) return NotFound();
            var user = _userService.GetUserById(id.Value);
            return user == null ? NotFound() : View(user);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            ViewData["DepartmentId"] = new SelectList(_departmentService.GetAllDepartments(), "Id", "Name");
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Id,FirstName,LastName,Email,PasswordHash,Role,DepartmentId,PhoneNumber,EmailNotificationsEnabled,PushNotificationsEnabled")] User user)
        {
            ModelState.Remove("Department");
            if (ModelState.IsValid)
            {
                _userService.CreateUser(user);
                return RedirectToAction(nameof(Index));
            }
            ViewData["DepartmentId"] = new SelectList(_departmentService.GetAllDepartments(), "Id", "Name", user.DepartmentId);
            return View(user);
        }

        // GET: Users/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null) return NotFound();
            var user = _userService.GetUserById(id.Value);
            if (user == null) return NotFound();

            ViewData["DepartmentId"] = new SelectList(_departmentService.GetAllDepartments(), "Id", "Name", user.DepartmentId);
            return View(user);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("Id,FirstName,LastName,Email,PasswordHash,Role,DepartmentId,PhoneNumber,EmailNotificationsEnabled,PushNotificationsEnabled")] User user)
        {
            if (id != user.Id) return NotFound();

            ModelState.Remove("Department");
            if (string.IsNullOrEmpty(user.PasswordHash)) ModelState.Remove("PasswordHash");

            if (ModelState.IsValid)
            {
                try
                {
                    _userService.UpdateUser(user, user.PasswordHash);
                }
                catch
                {
                    if (!_userService.UserExists(user.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["DepartmentId"] = new SelectList(_departmentService.GetAllDepartments(), "Id", "Name", user.DepartmentId);
            return View(user);
        }

        // GET: Users/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null) return NotFound();
            var user = _userService.GetUserById(id.Value);
            return user == null ? NotFound() : View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _userService.DeleteUser(id);
            return RedirectToAction(nameof(Index));
        }
    }
}