using IT_Helpdesk_Platform.Models;
using IT_Helpdesk_Platform.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IT_Helpdesk_Platform.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private readonly ITicketService _ticketService;
        private readonly ICategoryService _categoryService;
        private readonly UserManager<User> _userManager;

        public TicketsController(ITicketService ticketService, ICategoryService categoryService, UserManager<User> userManager)
        {
            _ticketService = ticketService;
            _categoryService = categoryService;
            _userManager = userManager;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(string? searchString, string? statusFilter, string? priorityFilter)
        {
            var tickets = await _ticketService.GetFilteredTicketsAsync(null, searchString, statusFilter, priorityFilter);
            SetViewDataFilters(searchString, statusFilter, priorityFilter);
            return View(tickets);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var ticket = await _ticketService.GetTicketDetailsAsync(id.Value);
            return ticket == null ? NotFound() : View(ticket);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            await PopulateDropdownsAsync();
            return View(new Ticket { Status = "Open", Priority = "Medium" });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Ticket ticket, List<IFormFile> Attachments)
        {
            ClearTicketModelState();

            if (ModelState.IsValid)
            {
                ticket.CreatedAt = DateTime.UtcNow;

                await _ticketService.CreateTicketAsync(ticket);

                await _ticketService.ProcessAttachmentsAsync(ticket.Id, Attachments);

                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdownsAsync(ticket);
            return View(ticket);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var ticket = await _ticketService.GetTicketDetailsAsync(id.Value);
            if (ticket == null) return NotFound();

            await PopulateDropdownsAsync(ticket);
            return View(ticket);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Status,Priority,CreatedAt,DueDate,CategoryId,RequesterId,AssignedAgentId")] Ticket ticket)
        {
            if (id != ticket.Id) return NotFound();
            ClearTicketModelState();

            if (ModelState.IsValid)
            {
                try
                {
                    await _ticketService.UpdateTicketAsync(ticket);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_ticketService.TicketExists(ticket.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            await PopulateDropdownsAsync(ticket);
            return View(ticket);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var ticket = await _ticketService.GetTicketDetailsAsync(id.Value);
            return ticket == null ? NotFound() : View(ticket);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _ticketService.DeleteTicketAsync(id);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> MyTickets(string searchString, string statusFilter, string priorityFilter)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var tickets = await _ticketService.GetFilteredTicketsAsync(currentUser?.Id ?? 0, searchString, statusFilter, priorityFilter);

            SetViewDataFilters(searchString, statusFilter, priorityFilter);
            return View(tickets);
        }

        [Authorize(Roles = "Admin,Agent")]
        public async Task<IActionResult> AllTickets(string searchString, string statusFilter, string priorityFilter)
        {
            var tickets = await _ticketService.GetFilteredTicketsAsync(null, searchString, statusFilter, priorityFilter);
            SetViewDataFilters(searchString, statusFilter, priorityFilter);
            return View(tickets);
        }

        [HttpGet]
        public async Task<IActionResult> SubmitTicket()
        {
            await PopulateDropdownsAsync();
            return View(new SubmitTicketViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitTicket(SubmitTicketViewModel model)
        {
            ModelState.Remove("Attachments");
            ModelState.Remove("UploadsBrute");

            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);

                var ticket = new Ticket
                {
                    Title = model.Title,
                    Description = model.Description,
                    CategoryId = model.CategoryId,
                    Priority = model.Priority,
                    Status = "Open",
                    CreatedAt = DateTime.UtcNow,
                    RequesterId = currentUser?.Id ?? 0
                };

                await _ticketService.CreateTicketAsync(ticket, isUserSubmission: true);

                await _ticketService.ProcessAttachmentsAsync(ticket.Id, Request.Form.Files);

                return RedirectToAction(nameof(MyTickets));
            }
            await PopulateDropdownsAsync();
            return View(model);
        }

        public async Task<IActionResult> ViewTicket(int id)
        {
            var ticket = await _ticketService.GetTicketDetailsAsync(id);
            if (ticket == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser?.Role != "Admin" && currentUser?.Role != "Agent" && ticket.RequesterId != currentUser?.Id)
            {
                return RedirectToAction("UserDashboard", "Home");
            }

            var supportStaff = await _ticketService.GetSupportStaffAsync();
            var staffList = supportStaff.Select(u => new { Id = u.Id, FullName = u.FirstName + " " + u.LastName }).ToList();

            ViewBag.Agents = new SelectList(staffList, "Id", "FullName");

            return View(ticket);
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(int ticketId, string text)
        {
            var files = Request.HasFormContentType ? Request.Form.Files : null;

            if (string.IsNullOrWhiteSpace(text) && (files == null || files.Count == 0))
                return Json(new { success = false, message = "Comment or attachment cannot be empty." });

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { success = false, message = "User not found." });

            var ticket = await _ticketService.GetTicketDetailsAsync(ticketId);
            if (ticket == null) return Json(new { success = false, message = "Ticket not found." });

            if (user.Role != "Admin" && user.Role != "Agent" && ticket.RequesterId != user.Id)
            {
                return Json(new { success = false, message = "Unauthorized: You cannot comment on someone else's ticket." });
            }

            var comment = new Comment
            {
                TicketId = ticketId,
                UserId = user.Id,
                Text = text ?? "",
                CreatedAt = DateTime.Now
            };

            await _ticketService.AddCommentAsync(comment);

            var uploadedFilePaths = new List<string>();

            if (files != null && files.Count > 0)
            {
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
                            CommentId = comment.Id
                        };

                        await _ticketService.AddAttachmentAsync(attachment);
                        uploadedFilePaths.Add(attachment.FilePath);
                    }
                }
            }

            string validProfilePic = "";
            if (!string.IsNullOrEmpty(user.ProfilePicturePath))
            {
                string picPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfilePicturePath.TrimStart('/'));
                if (System.IO.File.Exists(picPath))
                {
                    validProfilePic = user.ProfilePicturePath;
                }
            }

            return Json(new
            {
                success = true,
                userName = $"{(user.FirstName ?? "")} {(user.LastName ?? "")}".Trim(),
                date = comment.CreatedAt.ToString("MMM dd, yyyy, HH:mm"),
                text = comment.Text ?? "",
                role = user.Role ?? "User",
                profilePic = validProfilePic,
                isOwn = true,
                attachments = uploadedFilePaths
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Agent")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTicketStatus(int TicketId, string Status, string Priority, int? AssignedAgentId)
        {
            var ticket = await _ticketService.GetTicketDetailsAsync(TicketId);
            if (ticket == null) return NotFound();

            ticket.Status = Status;
            ticket.Priority = Priority;
            ticket.AssignedAgentId = AssignedAgentId;

            ticket.Requester = null!;
            ticket.AssignedAgent = null!;
            ticket.Category = null!;
            ticket.Comments = null!;
            ticket.Attachments = null!;

            await _ticketService.UpdateTicketAsync(ticket);
            TempData["SuccessMessage"] = "Ticket properties updated successfully.";

            return RedirectToAction("ViewTicket", new { id = TicketId });
        }

        private void ClearTicketModelState()
        {
            ModelState.Remove("Category");
            ModelState.Remove("Requester");
            ModelState.Remove("AssignedAgent");
            ModelState.Remove("Comments");
            ModelState.Remove("Attachments");
        }

        private async Task PopulateDropdownsAsync(Ticket? ticket = null)
        {
            var supportStaff = await _ticketService.GetSupportStaffAsync();
            var allUsers = await _ticketService.GetAllUsersAsync();

            ViewData["AgentList"] = new SelectList(supportStaff.Select(u => new { u.Id, u.Email }), "Id", "Email", ticket?.AssignedAgentId);
            ViewData["RequesterList"] = new SelectList(allUsers.Select(u => new { u.Id, u.Email }), "Id", "Email", ticket?.RequesterId);

            var categories = _categoryService.GetAllCategories().Select(c => new { c.Id, c.Name }).ToList();
            ViewData["CategoryList"] = new SelectList(categories, "Id", "Name", ticket?.CategoryId);
        }

        private void SetViewDataFilters(string? search, string? status, string? priority)
        {
            ViewData["CurrentFilter"] = search;
            ViewData["StatusFilter"] = status;
            ViewData["PriorityFilter"] = priority;
        }
    }
}