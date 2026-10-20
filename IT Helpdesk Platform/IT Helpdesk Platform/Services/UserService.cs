using IT_Helpdesk_Platform.Models;
using IT_Helpdesk_Platform.Repositories.Interfaces;
using IT_Helpdesk_Platform.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace IT_Helpdesk_Platform.Services
{
    public class UserService : IUserService
    {
        private readonly IRepositoryWrapper _wrapper;

        public UserService(IRepositoryWrapper wrapper) => _wrapper = wrapper;

        public User? Authenticate(string email, string password) =>
            _wrapper.User.GetUserByEmailAndPassword(email, password);

        public (bool Success, string Message) RegisterNewUser(string firstName, string lastName, int departmentId, string email, string password, string confirmPassword)
        {
            if (password != confirmPassword) return (false, "Passwords do not match!");
            if (_wrapper.User.EmailExists(email)) return (false, "This email is already in use.");

            var newUser = new User
            {
                FirstName = firstName,
                LastName = lastName,
                DepartmentId = departmentId,
                Email = email,
                PasswordHash = password,
                Role = "User"
            };

            _wrapper.User.Create(newUser);
            _wrapper.Save();

            return (true, "Account created successfully.");
        }

        public IEnumerable<User> GetAllUsers(string? searchString = null)
        {
            var query = _wrapper.User.FindAll().Include(u => u.Department).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(u => u.FirstName!.Contains(searchString)
                                      || u.LastName!.Contains(searchString)
                                      || u.Email!.Contains(searchString)
                                      || u.Id.ToString() == searchString);
            }

            return query.OrderBy(u => u.FirstName).ToList();
        }

        public IEnumerable<User> GetSupportStaff() =>
            _wrapper.User.FindAll().Where(u => u.Role == "Agent" || u.Role == "Admin").ToList();

        public User? GetUserById(int id) => _wrapper.User.GetUserByIdWithDetails(id);

        public void CreateUser(User user)
        {
            _wrapper.User.Create(user);
            _wrapper.Save();
        }

        public void UpdateUser(User user, string? newPassword = null)
        {
            if (string.IsNullOrEmpty(newPassword))
            {
                var existingUser = _wrapper.User.FindByCondition(u => u.Id == user.Id).FirstOrDefault();
                if (existingUser != null)
                {
                    user.PasswordHash = existingUser.PasswordHash;
                }
            }
            else
            {
                user.PasswordHash = newPassword;
            }

            _wrapper.User.Update(user);
            _wrapper.Save();
        }

        public void DeleteUser(int id)
        {
            var user = GetUserById(id);
            if (user != null)
            {
                _wrapper.User.Delete(user);
                _wrapper.Save();
            }
        }

        public bool UserExists(int id) => _wrapper.User.FindByCondition(u => u.Id == id).Any();
    }
}