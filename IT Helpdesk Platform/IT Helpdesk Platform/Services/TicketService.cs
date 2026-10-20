using IT_Helpdesk_Platform.Models;
using IT_Helpdesk_Platform.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IT_Helpdesk_Platform.Services
{
    public class TicketService : Interfaces.ITicketService
    {
        private readonly IRepositoryWrapper _wrapper;
        private readonly HelpdeskContext _context;
        private readonly UserManager<User> _userManager;

        public TicketService(IRepositoryWrapper wrapper, HelpdeskContext context, UserManager<User> userManager)
        {
            _wrapper = wrapper;
            _context = context;
            _userManager = userManager;
        }

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
            _context.ChangeTracker.Clear();

            var ticket = await _context.Tickets
                .Include(t => t.Comments)
                    .ThenInclude(c => c.Attachments)
                .Include(t => t.Attachments)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket != null)
            {
                _context.Tickets.Remove(ticket);
                await _context.SaveChangesAsync();
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

        public async Task AddCommentAsync(Comment comment)
        {
            _wrapper.Comment.Create(comment);
            await _wrapper.SaveAsync();
        }

        public async Task AddAttachmentAsync(Attachment attachment)
        {
            _wrapper.Attachment.Create(attachment);
            await _wrapper.SaveAsync();
        }

        public async Task<List<string>> ProcessAttachmentsAsync(int ticketId, IEnumerable<IFormFile> files, int? commentId = null)
        {
            var uploadedFilePaths = new List<string>();
            if (files == null || !files.Any()) return uploadedFilePaths;

            string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "tickets");
            if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
                    string filePath = Path.Combine(uploadFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    var attachment = new Attachment
                    {
                        FileName = file.FileName,
                        FilePath = "/uploads/tickets/" + uniqueFileName,
                        TicketId = ticketId,
                        CommentId = commentId,
                        CreatedAt = DateTime.UtcNow
                    };

                    _wrapper.Attachment.Create(attachment);
                    uploadedFilePaths.Add(attachment.FilePath);
                }
            }
            await _wrapper.SaveAsync();
            return uploadedFilePaths;
        }

        public async Task<IEnumerable<User>> GetSupportStaffAsync()
        {
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            var agents = await _userManager.GetUsersInRoleAsync("Agent");
            return admins.Concat(agents).DistinctBy(u => u.Id).ToList();
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _userManager.Users.ToListAsync();
        }
    }
}