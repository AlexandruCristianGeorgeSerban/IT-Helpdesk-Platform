using IT_Helpdesk_Platform.Models;
using System.Linq;
using System.Threading.Tasks;

namespace IT_Helpdesk_Platform.Repositories.Interfaces
{
    public interface ITicketRepository : IRepositoryBase<Ticket>
    {
        IQueryable<Ticket> GetQueryableTicketsWithDetails();
        Task<Ticket?> GetTicketByIdAsync(int id);
        bool TicketExists(int id);
    }
}