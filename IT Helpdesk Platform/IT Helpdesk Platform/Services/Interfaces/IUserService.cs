using IT_Helpdesk_Platform.Models;
using System.Collections.Generic;

namespace IT_Helpdesk_Platform.Services.Interfaces
{
    public interface IUserService
    {
        User? Authenticate(string email, string password);
        (bool Success, string Message) RegisterNewUser(string firstName, string lastName, int departmentId, string email, string password, string confirmPassword);

        IEnumerable<User> GetAllUsers(string? searchString = null);
        IEnumerable<User> GetSupportStaff();
        User? GetUserById(int id);
        void CreateUser(User user);
        void UpdateUser(User user, string? newPassword = null);
        void DeleteUser(int id);
        bool UserExists(int id);
    }
}