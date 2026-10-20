using IT_Helpdesk_Platform.Models;

namespace IT_Helpdesk_Platform.Repositories.Interfaces
{
    public interface IUserRepository : IRepositoryBase<User>
    {
        User? GetUserByEmailAndPassword(string email, string passwordHash);
        bool EmailExists(string email);
        User? GetUserByIdWithDetails(int id);
    }
}