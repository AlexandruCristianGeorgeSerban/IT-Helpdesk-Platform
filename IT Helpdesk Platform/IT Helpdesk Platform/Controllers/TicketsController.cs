using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using IT_Helpdesk_Platform.Models;

namespace IT_Helpdesk_Platform.Controllers
{
    public class TicketsController : Controller
    {
        private readonly HelpdeskContext _context;

        public TicketsController(HelpdeskContext context)
        {
            _context = context;
        }

        // GET: Tickets
        public async Task<IActionResult> Index()
        {
            var helpdeskContext = _context.Tickets.Include(t => t.AssignedAgent).Include(t => t.Category).Include(t => t.Requester);
            return View(await helpdeskContext.ToListAsync());
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.AssignedAgent)
                .Include(t => t.Category)
                .Include(t => t.Requester)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // GET: Tickets/Create
        public IActionResult Create()
        {
            var supportStaff = _context.Users.Where(u => u.Role == "Agent" || u.Role == "Admin").ToList();

            ViewData["AssignedAgentId"] = new SelectList(supportStaff, "Id", "Email");
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            ViewData["RequesterId"] = new SelectList(_context.Users, "Id", "Email"); // Oricine poate cere ajutor
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Status,Priority,CreatedAt,CategoryId,RequesterId,AssignedAgentId")] Ticket ticket)
        {
            ModelState.Remove("Category");
            ModelState.Remove("Requester");
            ModelState.Remove("AssignedAgent");

            if (ticket.CreatedAt == default)
            {
                ticket.CreatedAt = DateTime.Now;
            }

            if (ModelState.IsValid)
            {
                _context.Add(ticket);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var supportStaff = _context.Users.Where(u => u.Role == "Agent" || u.Role == "Admin").ToList();
            ViewData["AssignedAgentId"] = new SelectList(supportStaff, "Id", "Email", ticket.AssignedAgentId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", ticket.CategoryId);
            ViewData["RequesterId"] = new SelectList(_context.Users, "Id", "Email", ticket.RequesterId);
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null) return NotFound();

            var supportStaff = _context.Users.Where(u => u.Role == "Agent" || u.Role == "Admin").ToList();
            ViewData["AssignedAgentId"] = new SelectList(supportStaff, "Id", "Email", ticket.AssignedAgentId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", ticket.CategoryId);
            ViewData["RequesterId"] = new SelectList(_context.Users, "Id", "Email", ticket.RequesterId);
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Status,Priority,CreatedAt,CategoryId,RequesterId,AssignedAgentId")] Ticket ticket)
        {
            if (id != ticket.Id) return NotFound();

            ModelState.Remove("Category");
            ModelState.Remove("Requester");
            ModelState.Remove("AssignedAgent");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticket.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            var supportStaff = _context.Users.Where(u => u.Role == "Agent" || u.Role == "Admin").ToList();
            ViewData["AssignedAgentId"] = new SelectList(supportStaff, "Id", "Email", ticket.AssignedAgentId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", ticket.CategoryId);
            ViewData["RequesterId"] = new SelectList(_context.Users, "Id", "Email", ticket.RequesterId);
            return View(ticket);
        }

        // GET: Tickets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.AssignedAgent)
                .Include(t => t.Category)
                .Include(t => t.Requester)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket != null)
            {
                _context.Tickets.Remove(ticket);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TicketExists(int id)
        {
            return _context.Tickets.Any(e => e.Id == id);
        }
    }
}
