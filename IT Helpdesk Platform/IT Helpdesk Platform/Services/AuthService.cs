using IT_Helpdesk_Platform.Models;
using IT_Helpdesk_Platform.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace IT_Helpdesk_Platform.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly HelpdeskContext _context;
        private readonly IdentityOptions _identityOptions;

        public AuthService(UserManager<User> userManager, HelpdeskContext context, IOptions<IdentityOptions> identityOptions)
        {
            _userManager = userManager;
            _context = context;
            _identityOptions = identityOptions.Value;
        }

        public async Task<int?> GetLockoutMinutesLeftAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return null;

            var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
            if (lockoutEnd.HasValue)
            {
                var timeLeft = lockoutEnd.Value - DateTimeOffset.UtcNow;
                return (int)Math.Ceiling(timeLeft.TotalMinutes);
            }
            return null;
        }

        public async Task<int?> GetAttemptsLeftAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return null;

            int failedCount = await _userManager.GetAccessFailedCountAsync(user);
            int maxAttempts = _identityOptions.Lockout.MaxFailedAccessAttempts;

            return maxAttempts - failedCount;
        }

        public async Task<User?> GetUserProfileWithDetailsAsync(string email)
        {
            return await _context.Users
                                 .Include(u => u.Department)
                                 .Include(u => u.SubmittedTickets)
                                 .Include(u => u.AssignedTickets)
                                 .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> UpdateProfileAsync(int id, string firstName, string lastName, string phoneNumber, bool emailNotifications, bool pushNotifications, IFormFile? profilePicture)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return false;

            user.FirstName = firstName;
            user.LastName = lastName;
            user.PhoneNumber = phoneNumber;
            user.EmailNotificationsEnabled = emailNotifications;
            user.PushNotificationsEnabled = pushNotifications;

            if (profilePicture != null && profilePicture.Length > 0)
            {
                if (!string.IsNullOrEmpty(user.ProfilePicturePath))
                {
                    string oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfilePicturePath.TrimStart('/'));
                    if (File.Exists(oldFilePath)) File.Delete(oldFilePath);
                }

                string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
                if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(profilePicture.FileName);
                string filePath = Path.Combine(uploadFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await profilePicture.CopyToAsync(fileStream);
                }

                user.ProfilePicturePath = "/uploads/avatars/" + uniqueFileName;
            }

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<(bool Success, string[] Errors, User? User)> RegisterUserAsync(string firstName, string lastName, int departmentId, string email, string password)
        {
            var newUser = new User
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                DepartmentId = departmentId,
                Role = "User",
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(newUser, password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToArray();
                return (false, errors, null);
            }

            await _userManager.AddToRoleAsync(newUser, "User");
            return (true, Array.Empty<string>(), newUser);
        }

        public async Task<bool> VerifyAccountAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return false;

            user.EmailConfirmed = true;
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }
    }
}