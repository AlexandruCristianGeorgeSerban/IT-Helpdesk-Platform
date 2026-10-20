using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using IT_Helpdesk_Platform.Models;
using IT_Helpdesk_Platform.Services.Interfaces;

namespace IT_Helpdesk_Platform.Controllers
{
    public class TicketsController : Controller
    {
        private readonly ITicketService _ticketService;
        private readonly ICategoryService _categoryService;
        private readonly IUserService _userService;

        public TicketsController(ITicketService ticketService, ICategoryService categoryService, IUserService userService)
        {
            _ticketService = ticketService;
            _categoryService = categoryService;
            _userService = userService;
        }

        public async Task<IActionResult> Index(string? searchString, string? statusFilter, string? priorityFilter)
        {
            var tickets = await _ticketService.GetFilteredTicketsAsync(null, searchString, statusFilter, priorityFilter);

            SetViewDataFilters(searchString, statusFilter, priorityFilter);

            return View(tickets);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var ticket = await _ticketService.GetTicketDetailsAsync(id.Value);
            return ticket == null ? NotFound() : View(ticket);
        }

        public IActionResult Create()
        {
            PopulateDropdowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Status,Priority,CreatedAt,CategoryId,RequesterId,AssignedAgentId")] Ticket ticket)
        {
            ClearTicketModelState();
            if (ModelState.IsValid)
            {
                await _ticketService.CreateTicketAsync(ticket);
                return RedirectToAction(nameof(Index));
            }
            PopulateDropdowns(ticket);
            return View(ticket);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var ticket = await _ticketService.GetTicketDetailsAsync(id.Value);
            if (ticket == null) return NotFound();

            PopulateDropdowns(ticket);
            return View(ticket);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Status,Priority,CreatedAt,CategoryId,RequesterId,AssignedAgentId")] Ticket ticket)
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
            PopulateDropdowns(ticket);
            return View(ticket);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var ticket = await _ticketService.GetTicketDetailsAsync(id.Value);
            return ticket == null ? NotFound() : View(ticket);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _ticketService.DeleteTicketAsync(id);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> MyTickets(string searchString, string statusFilter, string priorityFilter)
        {
            int currentUserId = 1; // Simulating user with ID 1 is logged in
            var tickets = await _ticketService.GetFilteredTicketsAsync(currentUserId, searchString, statusFilter, priorityFilter);
            SetViewDataFilters(searchString, statusFilter, priorityFilter);
            return View(tickets);
        }

        public async Task<IActionResult> AllTickets(string searchString, string statusFilter, string priorityFilter)
        {
            var tickets = await _ticketService.GetFilteredTicketsAsync(null, searchString, statusFilter, priorityFilter);
            SetViewDataFilters(searchString, statusFilter, priorityFilter);
            return View(tickets);
        }

        public IActionResult SubmitTicket()
        {
            PopulateDropdowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitTicket([Bind("Title,Description,Priority,CategoryId")] Ticket ticket)
        {
            ticket.RequesterId = 1; // Milestone 3 placeholder User ID = 1

            ticket.Status = "Open";

            ticket.CreatedAt = DateTime.Now;

            ModelState.Remove("RequesterId");
            ModelState.Remove("Status");
            ModelState.Remove("Requester");

            if (ModelState.IsValid)
            {
                await _ticketService.CreateTicketAsync(ticket, isUserSubmission: true);

                TempData["SuccessMessage"] = $"Your ticket '{ticket.Title}' was successfully created!";
                return RedirectToAction(nameof(MyTickets));
            }

            PopulateDropdowns(ticket);
            return View(ticket);
        }

        public async Task<IActionResult> ViewTicket(int id)
        {
            var ticket = await _ticketService.GetTicketDetailsAsync(id);
            if (ticket == null) return NotFound();

            var supportStaff = _userService.GetSupportStaff();

            var agents = supportStaff.Select(u => new
            {
                Id = u.Id,
                FullName = u.FirstName + " " + u.LastName
            }).ToList();

            ViewBag.Agents = new SelectList(agents, "Id", "FullName");

            return View(ticket);
        }

        private void ClearTicketModelState()
        {
            ModelState.Remove("Category");
            ModelState.Remove("Requester");
            ModelState.Remove("AssignedAgent");
        }

        private void PopulateDropdowns(Ticket? ticket = null)
        {
            ViewData["AssignedAgentId"] = new SelectList(_userService.GetSupportStaff(), "Id", "Email", ticket?.AssignedAgentId);
            ViewData["CategoryId"] = new SelectList(_categoryService.GetAllCategories(), "Id", "Name", ticket?.CategoryId);
            ViewData["RequesterId"] = new SelectList(_userService.GetAllUsers(), "Id", "Email", ticket?.RequesterId);
        }

        private void SetViewDataFilters(string? search, string? status, string? priority)
        {
            ViewData["CurrentFilter"] = search;
            ViewData["StatusFilter"] = status;
            ViewData["PriorityFilter"] = priority;
        }
    }
}