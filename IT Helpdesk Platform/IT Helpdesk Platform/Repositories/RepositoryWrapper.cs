using IT_Helpdesk_Platform.Models;
using IT_Helpdesk_Platform.Repositories;
using IT_Helpdesk_Platform.Repositories.Interfaces;

public class RepositoryWrapper : IRepositoryWrapper
{
    private HelpdeskContext _context;
    private ITicketRepository? _ticket;
    private IDepartmentRepository? _department;
    private ICategoryRepository? _category;
    private IKnowledgeBaseRepository? _knowledgeBase;
    private ICommentRepository? _comment;
    private IAttachmentRepository? _attachment;

    public RepositoryWrapper(HelpdeskContext context)
    {
        _context = context;
    }

    public ITicketRepository Ticket => _ticket ??= new TicketRepository(_context);
    public IDepartmentRepository Department => _department ??= new DepartmentRepository(_context);
    public ICategoryRepository Category => _category ??= new CategoryRepository(_context);
    public IKnowledgeBaseRepository KnowledgeBase => _knowledgeBase ??= new KnowledgeBaseRepository(_context);
    public ICommentRepository Comment => _comment ??= new CommentRepository(_context);
    public IAttachmentRepository Attachment => _attachment ??= new AttachmentRepository(_context);

    public void Save() => _context.SaveChanges();
    public async Task SaveAsync() => await _context.SaveChangesAsync();
}