using System.Threading.Tasks;

namespace IT_Helpdesk_Platform.Repositories.Interfaces
{
    public interface IRepositoryWrapper
    {
        IUserRepository User { get; }
        ITicketRepository Ticket { get; }
        IDepartmentRepository Department { get; }
        ICategoryRepository Category { get; }
        IKnowledgeBaseRepository KnowledgeBase { get; }

        void Save();
        Task SaveAsync();
    }
}