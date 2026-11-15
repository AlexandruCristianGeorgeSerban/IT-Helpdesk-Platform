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
                "Critical" => DateTime.UtcNow.AddHours(4),
                "High" => DateTime.UtcNow.AddHours(24),
                "Medium" => DateTime.UtcNow.AddDays(3),
                "Low" => DateTime.UtcNow.AddDays(7),
                _ => DateTime.UtcNow.AddDays(3)
            };

            // Smart Agent Routing: auto-assign ticket to least-loaded agent if unassigned
            if (ticket.AssignedAgentId == null || ticket.AssignedAgentId == 0)
            {
                var leastLoadedAgent = await GetLeastLoadedAgentAsync(ticket.CategoryId);
                if (leastLoadedAgent != null)
                {
                    ticket.AssignedAgentId = leastLoadedAgent.Id;
                }
            }

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
            var staffIds = admins.Concat(agents).Select(u => u.Id).Distinct().ToList();

            return await _context.Users
                .Include(u => u.Department)
                .Where(u => staffIds.Contains(u.Id))
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _userManager.Users.ToListAsync();
        }

        public async Task<int> CalculateAgentWorkloadScoreAsync(int agentId)
        {
            var activeTickets = await _context.Tickets
                .Where(t => t.AssignedAgentId == agentId &&
                            t.Status != null &&
                            t.Status.ToLower() != "resolved" &&
                            t.Status.ToLower() != "closed")
                .Select(t => t.Priority)
                .ToListAsync();

            return activeTickets.Sum(priority => (priority ?? "").ToLower() switch
            {
                "critical" => 4,
                "high" => 3,
                "medium" => 2,
                "low" => 1,
                _ => 1
            });
        }

        public async Task<Dictionary<int, int>> GetStaffWorkloadScoresAsync()
        {
            var staff = await GetSupportStaffAsync();
            var scores = new Dictionary<int, int>();

            var activeTickets = await _context.Tickets
                .Where(t => t.AssignedAgentId.HasValue &&
                            t.Status != null &&
                            t.Status.ToLower() != "resolved" &&
                            t.Status.ToLower() != "closed")
                .Select(t => new { AgentId = t.AssignedAgentId!.Value, Priority = t.Priority ?? "" })
                .ToListAsync();

            var groupedScores = activeTickets
                .GroupBy(t => t.AgentId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(t => t.Priority.ToLower() switch
                    {
                        "critical" => 4,
                        "high" => 3,
                        "medium" => 2,
                        "low" => 1,
                        _ => 1
                    })
                );

            foreach (var member in staff)
            {
                scores[member.Id] = groupedScores.GetValueOrDefault(member.Id, 0);
            }

            return scores;
        }

        public async Task<User?> GetLeastLoadedAgentAsync(int? categoryId = null)
        {
            var allStaff = (await GetSupportStaffAsync()).ToList();

            // Automatically assign exclusively to Support Agents (Role == "Agent"), never to Administrators
            var agents = allStaff.Where(u => string.Equals(u.Role, "Agent", StringComparison.OrdinalIgnoreCase)).ToList();
            if (!agents.Any())
            {
                agents = allStaff; // Fallback only if no dedicated agents exist
            }
            if (!agents.Any()) return null;

            var workloads = await GetStaffWorkloadScoresAsync();

            // Try to match category domain with agent's department if available
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                var category = await _context.Categories.FindAsync(categoryId.Value);
                if (category?.Name != null)
                {
                    var matchingStaff = agents.Where(u =>
                        u.Department != null &&
                        (u.Department.Name!.Contains(category.Name, StringComparison.OrdinalIgnoreCase) ||
                         category.Name.Contains(u.Department.Name, StringComparison.OrdinalIgnoreCase))
                    ).ToList();

                    if (matchingStaff.Any())
                    {
                        return matchingStaff.OrderBy(u => workloads.GetValueOrDefault(u.Id, 0)).First();
                    }
                }
            }

            // Fallback: return the support agent with the minimum workload score overall
            return agents.OrderBy(u => workloads.GetValueOrDefault(u.Id, 0)).First();
        }
    }
}