namespace IT_Helpdesk_Platform.Repositories.Interfaces
{
    public interface IRepositoryWrapper
    {
        ITicketRepository Ticket { get; }
        IDepartmentRepository Department { get; }
        ICategoryRepository Category { get; }
        IKnowledgeBaseRepository KnowledgeBase { get; }
        ICommentRepository Comment { get; }
        IAttachmentRepository Attachment { get; }

        void Save();
        Task SaveAsync();
    }
}