using IT_Helpdesk_Platform.Models;
using IT_Helpdesk_Platform.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace IT_Helpdesk_Platform.Repositories
{
    public class UserRepository : RepositoryBase<User>, IUserRepository
    {
        public UserRepository(HelpdeskContext context) : base(context) { }

        public User? GetUserByEmailAndPassword(string email, string passwordHash) =>
            FindByCondition(u => u.Email == email && u.PasswordHash == passwordHash).FirstOrDefault();

        public bool EmailExists(string email) =>
            FindByCondition(u => u.Email == email).Any();

        public User? GetUserByIdWithDetails(int id) =>
            FindByCondition(u => u.Id == id)
                .Include(u => u.Department)
                .Include(u => u.SubmittedTickets)
                .Include(u => u.AssignedTickets)
                .FirstOrDefault();
    }
}