using IT_Helpdesk_Platform.Models;
using IT_Helpdesk_Platform.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IT_Helpdesk_Platform.Services
{
    public class TicketService : Interfaces.ITicketService
    {
        private readonly IRepositoryWrapper _wrapper;
        public TicketService(IRepositoryWrapper wrapper) => _wrapper = wrapper;

        public async Task<IEnumerable<Ticket>> GetAllTicketsAsync() =>
            await _wrapper.Ticket.GetQueryableTicketsWithDetails().ToListAsync();

        public async Task<Ticket?> GetTicketDetailsAsync(int id) =>
            await _wrapper.Ticket.GetTicketByIdAsync(id);

        public async Task CreateTicketAsync(Ticket ticket, bool isUserSubmission = false)
        {
            ticket.DueDate = ticket.Priority switch
            {
                "Critical" => DateTime.Now.AddHours(4),
                "High" => DateTime.Now.AddHours(24),
                "Medium" => DateTime.Now.AddDays(3),
                "Low" => DateTime.Now.AddDays(7),
                _ => DateTime.Now.AddDays(3)
            };

            _wrapper.Ticket.Create(ticket);
            await _wrapper.SaveAsync();
        }

        public async Task UpdateTicketAsync(Ticket ticket)
        {
            _wrapper.Ticket.Update(ticket);
            await _wrapper.SaveAsync();
        }

        public async Task DeleteTicketAsync(int id)
        {
            var ticket = await _wrapper.Ticket.GetTicketByIdAsync(id);
            if (ticket != null)
            {
                _wrapper.Ticket.Delete(ticket);
                await _wrapper.SaveAsync();
            }
        }

        public bool TicketExists(int id) => _wrapper.Ticket.TicketExists(id);

        public async Task<IEnumerable<Ticket>> GetFilteredTicketsAsync(int? requesterId, string? searchString, string? statusFilter, string? priorityFilter)
        {
            var query = _wrapper.Ticket.GetQueryableTicketsWithDetails();

            if (requesterId.HasValue) query = query.Where(t => t.RequesterId == requesterId.Value);

            if (!string.IsNullOrEmpty(searchString))
            {
                if (int.TryParse(searchString, out int searchId))
                    query = query.Where(t => t.Id == searchId
                                          || t.Title!.Contains(searchString)
                                          || t.Description!.Contains(searchString));
                else
                    query = query.Where(t => t.Title!.Contains(searchString)
                                          || t.Description!.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All") query = query.Where(t => t.Status == statusFilter);
            if (!string.IsNullOrEmpty(priorityFilter) && priorityFilter != "All") query = query.Where(t => t.Priority == priorityFilter);

            return await query.ToListAsync();
        }
    }
}