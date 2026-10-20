using IT_Helpdesk_Platform.Models;
using Microsoft.EntityFrameworkCore;

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
    }
}