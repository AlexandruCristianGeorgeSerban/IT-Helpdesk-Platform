using IT_Helpdesk_Platform.Models;
using IT_Helpdesk_Platform.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IT_Helpdesk_Platform.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly HelpdeskContext _context;

        public UserService(UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager, HelpdeskContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<IEnumerable<User>> GetFilteredUsersAsync(string? searchString)
        {
            var query = _userManager.Users.Include(u => u.Department).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(u => u.FirstName!.Contains(searchString)
                                      || u.LastName!.Contains(searchString)
                                      || u.Email!.Contains(searchString));
            }

            return await query.ToListAsync();
        }

        public async Task<User?> GetUserByIdWithDepartmentAsync(int id)
        {
            return await _userManager.Users
                                     .Include(u => u.Department)
                                     .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<IEnumerable<string>> GetUserRolesAsync(User user)
        {
            return await _userManager.GetRolesAsync(user);
        }

        public async Task<IEnumerable<string>> GetAllRolesAsync()
        {
            return await _roleManager.Roles.Select(r => r.Name!).ToListAsync();
        }

        public async Task<IdentityResult> UpdateUserWithRoleAsync(User formUser, string selectedRole, string? newPassword)
        {
            var existingUser = await _userManager.FindByIdAsync(formUser.Id.ToString());
            if (existingUser == null) return IdentityResult.Failed(new IdentityError { Description = "User not found" });

            existingUser.FirstName = formUser.FirstName;
            existingUser.LastName = formUser.LastName;
            existingUser.Email = formUser.Email;
            existingUser.UserName = formUser.Email;
            existingUser.PhoneNumber = formUser.PhoneNumber;
            existingUser.DepartmentId = formUser.DepartmentId;
            existingUser.Role = selectedRole;

            if (!string.IsNullOrEmpty(newPassword))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(existingUser);
                var passResult = await _userManager.ResetPasswordAsync(existingUser, token, newPassword);
                if (!passResult.Succeeded) return passResult;
            }

            var updateResult = await _userManager.UpdateAsync(existingUser);
            if (!updateResult.Succeeded) return updateResult;

            var currentRoles = await _userManager.GetRolesAsync(existingUser);
            if (!currentRoles.Contains(selectedRole))
            {
                await _userManager.RemoveFromRolesAsync(existingUser, currentRoles);
                await _userManager.AddToRoleAsync(existingUser, selectedRole);
            }

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> CreateUserWithRoleAsync(User user, string password, string role)
        {
            user.UserName = user.Email;
            user.Role = role;
            user.EmailConfirmed = true;

            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, role);
            }
            return result;
        }

        public async Task<IdentityResult> DeleteUserAsync(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return IdentityResult.Failed(new IdentityError { Description = "User not found" });

            if (!string.IsNullOrEmpty(user.ProfilePicturePath))
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfilePicturePath.TrimStart('/'));
                if (File.Exists(filePath)) File.Delete(filePath);
            }

            var assignedTickets = await _context.Tickets.Where(t => t.AssignedAgentId == id).ToListAsync();
            foreach (var ticket in assignedTickets) ticket.AssignedAgentId = null;

            var userComments = await _context.Comments
                .Include(c => c.Attachments)
                .Where(c => c.UserId == id)
                .ToListAsync();

            var submittedTickets = await _context.Tickets
                .Include(t => t.Attachments)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.Attachments)
                .Where(t => t.RequesterId == id)
                .ToListAsync();

            var allAttachmentsToDelete = userComments.SelectMany(c => c.Attachments!)
                .Concat(submittedTickets.SelectMany(t => t.Attachments!))
                .Concat(submittedTickets.SelectMany(t => t.Comments!).SelectMany(c => c.Attachments!))
                .ToList();

            foreach (var attachment in allAttachmentsToDelete)
            {
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", attachment.FilePath.TrimStart('/'));
                if (File.Exists(path)) File.Delete(path);
            }

            _context.Attachments.RemoveRange(allAttachmentsToDelete);
            _context.Comments.RemoveRange(submittedTickets.SelectMany(t => t.Comments));
            _context.Comments.RemoveRange(userComments);
            _context.Tickets.RemoveRange(submittedTickets);

            await _context.SaveChangesAsync();

            return await _userManager.DeleteAsync(user);
        }
    }
}