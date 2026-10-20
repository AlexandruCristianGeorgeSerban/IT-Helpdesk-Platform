using IT_Helpdesk_Platform.Models;
using IT_Helpdesk_Platform.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace IT_Helpdesk_Platform.Repositories
{
    public class TicketRepository : RepositoryBase<Ticket>, ITicketRepository
    {
        public TicketRepository(HelpdeskContext context) : base(context) { }

        public IQueryable<Ticket> GetQueryableTicketsWithDetails() =>
            FindAll()
                .Include(t => t.AssignedAgent)
                .Include(t => t.Category)
                .Include(t => t.Requester)
                .ThenInclude(u => u!.Department);

        public async Task<Ticket?> GetTicketByIdAsync(int id) =>
            await GetQueryableTicketsWithDetails().FirstOrDefaultAsync(t => t.Id == id);

        public bool TicketExists(int id) => FindByCondition(t => t.Id == id).Any();
    }
}