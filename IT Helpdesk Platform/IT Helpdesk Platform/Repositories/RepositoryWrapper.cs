using IT_Helpdesk_Platform.Models;
using IT_Helpdesk_Platform.Repositories.Interfaces;
using System.Threading.Tasks;

namespace IT_Helpdesk_Platform.Repositories
{
    public class RepositoryWrapper : IRepositoryWrapper
    {
        private HelpdeskContext _context;
        private IUserRepository? _user;
        private ITicketRepository? _ticket;
        private IDepartmentRepository? _department;
        private ICategoryRepository? _category;
        private IKnowledgeBaseRepository? _knowledgeBase;

        public RepositoryWrapper(HelpdeskContext context)
        {
            _context = context;
        }

        public IUserRepository User => _user ??= new UserRepository(_context);
        public ITicketRepository Ticket => _ticket ??= new TicketRepository(_context);
        public IDepartmentRepository Department => _department ??= new DepartmentRepository(_context);
        public ICategoryRepository Category => _category ??= new CategoryRepository(_context);
        public IKnowledgeBaseRepository KnowledgeBase => _knowledgeBase ??= new KnowledgeBaseRepository(_context);

        public void Save() => _context.SaveChanges();
        public async Task SaveAsync() => await _context.SaveChangesAsync();
    }
}