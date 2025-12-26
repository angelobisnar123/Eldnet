using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1_Test1.Data;
using WebApplication1_Test1.Models;

namespace WebApplication1_Test1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                if (User.Identity.Name == "administrator123@admin.com")
                    return RedirectToAction("Index", "Admin");

                return RedirectToAction("Dashboard");
            }

            return View();
        }

        // Dashboard for logged-in users
        public async Task<IActionResult> Dashboard()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var today = DateTime.Today;

            var lockersActive = await _context.LockerReservations
                .CountAsync(l => l.UserId == user.Id && l.Status == "Approved" && l.EndDate >= today);

            var lockerPending = await _context.LockerReservations
                .CountAsync(l => l.UserId == user.Id && l.Status == "Pending");

            var activitiesUpcoming = await _context.ActivityReservations
                .CountAsync(a => a.UserId == user.Id && a.Status == "Approved" && a.ActivityDate >= today);

            var activityPending = await _context.ActivityReservations
                .CountAsync(a => a.UserId == user.Id && a.Status == "Pending");

            var gatePassesActive = await _context.GatePasses
                .CountAsync(g => g.UserId == user.Id && g.Status == "Approved" && g.ExitDate >= today);

            var gatePassPending = await _context.GatePasses
                .CountAsync(g => g.UserId == user.Id && g.Status == "Pending");

            var recentExpenses = await _context.Expenses
                .Where(e => e.UserId == user.Id)
                .OrderByDescending(e => e.Date)
                .Take(5)
                .ToListAsync();

            var model = new UserDashboardViewModel
            {
                LockersActive = lockersActive,
                ActivitiesUpcoming = activitiesUpcoming,
                GatePassesActive = gatePassesActive,
                LockerPending = lockerPending,
                ActivityPending = activityPending,
                GatePassPending = gatePassPending,
                RecentExpenses = recentExpenses
            };

            return View(model);
        }

        // Returns calendar events (JSON) for the logged-in user
        [HttpGet]
        public async Task<JsonResult> GetCalendarEvents()
        {
            if (!User.Identity.IsAuthenticated)
                return Json(new object[0]);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new object[0]);

            var events = new List<object>();

            // Activities (approved) -> time-based events
            var activities = await _context.ActivityReservations
                .Where(a => a.UserId == user.Id && a.Status == "Approved")
                .ToListAsync();

            foreach (var a in activities)
            {
                var start = a.ActivityDate.Date + a.StartTime;
                var end = a.ActivityDate.Date + a.EndTime;
                events.Add(new
                {
                    id = $"activity-{a.Id}",
                    title = $"?? {a.ActivityName}",
                    start = start.ToString("s"),
                    end = end.ToString("s"),
                    allDay = false,
                    type = "activity",
                    extendedProps = new { a.Description }
                });
            }

            // Gate passes (approved) -> show exit time to return (if present)
            var passes = await _context.GatePasses
                .Where(g => g.UserId == user.Id && g.Status == "Approved")
                .ToListAsync();

            foreach (var g in passes)
            {
                var start = g.ExitDate.Date + g.ExitTime;
                DateTime? end = null;
                if (g.ReturnDate.HasValue && g.ReturnTime.HasValue)
                    end = g.ReturnDate.Value.Date + g.ReturnTime.Value;

                events.Add(new
                {
                    id = $"gate-{g.Id}",
                    title = $"?? Gate: {g.Destination}",
                    start = start.ToString("s"),
                    end = end?.ToString("s"),
                    allDay = false,
                    type = "gatepass",
                    extendedProps = new { g.Reason }
                });
            }

            // Locker reservations (approved) -> all-day multi-day events
            var lockers = await _context.LockerReservations
                .Where(l => l.UserId == user.Id && l.Status == "Approved")
                .ToListAsync();

            foreach (var l in lockers)
            {
                // FullCalendar treats 'end' as exclusive; add one day to display inclusive end date
                var endExclusive = l.EndDate.AddDays(1);
                events.Add(new
                {
                    id = $"locker-{l.Id}",
                    title = $"?? Locker {l.LockerNumber}",
                    start = l.StartDate.ToString("yyyy-MM-dd"),
                    end = endExclusive.ToString("yyyy-MM-dd"),
                    allDay = true,
                    type = "locker",
                    extendedProps = new { l.Purpose }
                });
            }

            return Json(events);
        }
        // AJAX endpoint for dashboard stats updates
        [HttpGet]
        public async Task<JsonResult> DashboardStats()
        {
            if (!User.Identity.IsAuthenticated)
                return Json(new { success = false, message = "Not authenticated" });

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { success = false, message = "User not found" });

            var today = DateTime.Today;

            var lockersActive = await _context.LockerReservations
                .CountAsync(l => l.UserId == user.Id && l.Status == "Approved" && l.EndDate >= today);

            var lockerPending = await _context.LockerReservations
                .CountAsync(l => l.UserId == user.Id && l.Status == "Pending");

            var activitiesUpcoming = await _context.ActivityReservations
                .CountAsync(a => a.UserId == user.Id && a.Status == "Approved" && a.ActivityDate >= today);

            var activityPending = await _context.ActivityReservations
                .CountAsync(a => a.UserId == user.Id && a.Status == "Pending");

            var gatePassesActive = await _context.GatePasses
                .CountAsync(g => g.UserId == user.Id && g.Status == "Approved" && g.ExitDate >= today);

            var gatePassPending = await _context.GatePasses
                .CountAsync(g => g.UserId == user.Id && g.Status == "Pending");

            return Json(new
            {
                success = true,
                lockersActive,
                lockerPending,
                activitiesUpcoming,
                activityPending,
                gatePassesActive,
                gatePassPending
            });
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}