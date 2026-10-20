using IT_Helpdesk_Platform.Models;

namespace IT_Helpdesk_Platform.Services.Interfaces
{
    public interface IAuthService
    {
        Task<int?> GetLockoutMinutesLeftAsync(string email);
        Task<int?> GetAttemptsLeftAsync(string email);
        Task<User?> GetUserProfileWithDetailsAsync(string email);
        Task<bool> UpdateProfileAsync(int id, string firstName, string lastName, string phoneNumber, bool emailNotifications, bool pushNotifications, IFormFile? profilePicture);
        Task<(bool Success, string[] Errors, User? User)> RegisterUserAsync(string firstName, string lastName, int departmentId, string email, string password);
        Task<bool> VerifyAccountAsync(string email);
    }
}