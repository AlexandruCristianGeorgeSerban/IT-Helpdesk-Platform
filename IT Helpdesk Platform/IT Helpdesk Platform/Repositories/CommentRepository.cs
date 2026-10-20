using IT_Helpdesk_Platform.Models;
using IT_Helpdesk_Platform.Repositories.Interfaces;

namespace IT_Helpdesk_Platform.Repositories
{
    public class CommentRepository : RepositoryBase<Comment>, ICommentRepository
    {
        public CommentRepository(HelpdeskContext context) : base(context)
        {
        }
    }
}