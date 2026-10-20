using IT_Helpdesk_Platform.Models;

namespace IT_Helpdesk_Platform.Services.Interfaces
{
    public interface ITicketService
    {
        Task<IEnumerable<Ticket>> GetAllTicketsAsync();
        Task<Ticket?> GetTicketDetailsAsync(int id);
        Task CreateTicketAsync(Ticket ticket, bool isUserSubmission = false);
        Task UpdateTicketAsync(Ticket ticket);
        Task DeleteTicketAsync(int id);
        bool TicketExists(int id);
        Task<IEnumerable<Ticket>> GetFilteredTicketsAsync(int? requesterId, string? searchString, string? statusFilter, string? priorityFilter);
        Task AddCommentAsync(Comment comment);
        Task AddAttachmentAsync(Attachment attachment);
        Task<List<string>> ProcessAttachmentsAsync(int ticketId, IEnumerable<IFormFile> files, int? commentId = null);
        Task<IEnumerable<User>> GetSupportStaffAsync();
        Task<IEnumerable<User>> GetAllUsersAsync();
    }
}