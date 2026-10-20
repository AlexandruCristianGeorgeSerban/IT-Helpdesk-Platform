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

        public HomeController(ILogger<HomeController> logger, ITicketService ticketService)
        {
            _logger = logger;
            _ticketService = ticketService;
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
        public IActionResult UserDashboard()
        {
            return View();
        }

        // 3. Dashboard for IT agents
        [Authorize(Roles = "Admin,Agent")]
        public async Task<IActionResult> AgentDashboard()
        {
            var ticketsList = await _ticketService.GetAllTicketsAsync();
            return View(ticketsList);
        }

        // 4. Dashboard for administrators
        [Authorize(Roles = "Admin")]
        public IActionResult AdministratorDashboard()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}