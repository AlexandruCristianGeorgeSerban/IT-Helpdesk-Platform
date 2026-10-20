using IT_Helpdesk_Platform.Models;
using IT_Helpdesk_Platform.Repositories.Interfaces;

namespace IT_Helpdesk_Platform.Repositories
{
    public class AttachmentRepository : RepositoryBase<Attachment>, IAttachmentRepository
    {
        public AttachmentRepository(HelpdeskContext context) : base(context)
        {
        }
    }
}