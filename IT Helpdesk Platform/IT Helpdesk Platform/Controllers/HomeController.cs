using IT_Helpdesk_Platform.Models;
using IT_Helpdesk_Platform.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace IT_Helpdesk_Platform.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ITicketService _ticketService;
        private readonly Microsoft.AspNetCore.Identity.UserManager<User> _userManager;

        public HomeController(
            ILogger<HomeController> logger,
            ITicketService ticketService,
            Microsoft.AspNetCore.Identity.UserManager<User> userManager)
        {
            _logger = logger;
            _ticketService = ticketService;
            _userManager = userManager;
        }

        // 1. Dashboard for guests
        public IActionResult GuestDashboard()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Admin")) return RedirectToAction("AdministratorDashboard");
                if (User.IsInRole("Agent")) return RedirectToAction("AgentDashboard");

                return RedirectToAction("UserDashboard");
            }

            return View();
        }

        // 2. Dashboard for users
        [Authorize(Roles = "User")]
        public IActionResult UserDashboard()
        {
            return View();
        }

        // 3. Dashboard for IT agents
        [Authorize(Roles = "Admin, Agent")]
        public async Task<IActionResult> AgentDashboard()
        {
            var ticketsList = await _ticketService.GetAllTicketsAsync();
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                ViewBag.MyWorkloadScore = await _ticketService.CalculateAgentWorkloadScoreAsync(currentUser.Id);
            }
            return View(ticketsList);
        }

        // 4. Dashboard for administrators
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdministratorDashboard()
        {
            var tickets = (await _ticketService.GetAllTicketsAsync()).ToList();
            var currentUser = await _userManager.GetUserAsync(User);

            ViewBag.OpenTicketsCount = tickets.Count(t => string.Equals(t.Status, "Open", StringComparison.OrdinalIgnoreCase));
            ViewBag.InProgressCount = tickets.Count(t => string.Equals(t.Status, "In Progress", StringComparison.OrdinalIgnoreCase));
            ViewBag.SlaBreachedCount = tickets.Count(t => t.DueDate.HasValue &&
                                                          t.DueDate.Value < DateTime.UtcNow &&
                                                          !string.Equals(t.Status, "Resolved", StringComparison.OrdinalIgnoreCase) &&
                                                          !string.Equals(t.Status, "Closed", StringComparison.OrdinalIgnoreCase));

            if (currentUser != null)
            {
                ViewBag.MyWorkloadScore = await _ticketService.CalculateAgentWorkloadScoreAsync(currentUser.Id);
                ViewBag.MyActiveTicketsCount = tickets.Count(t => t.AssignedAgentId == currentUser.Id &&
                                                                   !string.Equals(t.Status, "Resolved", StringComparison.OrdinalIgnoreCase) &&
                                                                   !string.Equals(t.Status, "Closed", StringComparison.OrdinalIgnoreCase));
            }

            var staffWorkloads = await _ticketService.GetStaffWorkloadScoresAsync();
            ViewBag.TotalTeamWorkload = staffWorkloads.Values.Sum();

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}