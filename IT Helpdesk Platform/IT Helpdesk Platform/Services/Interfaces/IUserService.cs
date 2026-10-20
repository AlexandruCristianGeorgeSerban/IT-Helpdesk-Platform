using IT_Helpdesk_Platform.Models;
using Microsoft.AspNetCore.Identity;

namespace IT_Helpdesk_Platform.Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetFilteredUsersAsync(string? searchString);
        Task<User?> GetUserByIdWithDepartmentAsync(int id);
        Task<IEnumerable<string>> GetUserRolesAsync(User user);
        Task<IEnumerable<string>> GetAllRolesAsync();
        Task<IdentityResult> UpdateUserWithRoleAsync(User formUser, string selectedRole, string? newPassword);
        Task<IdentityResult> CreateUserWithRoleAsync(User user, string password, string role);
        Task<IdentityResult> DeleteUserAsync(int id);
    }
}