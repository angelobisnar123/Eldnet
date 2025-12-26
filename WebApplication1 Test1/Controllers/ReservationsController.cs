using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net;
using WebApplication1_Test1.Data;
using WebApplication1_Test1.Models;
using WebApplication1_Test1.Services;

namespace WebApplication1_Test1.Controllers
{
    [Authorize]
    public class ReservationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly SmtpOptions _smtpOptions;

        public ReservationsController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IEmailSender emailSender, IOptions<SmtpOptions> smtpOptions)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
            _smtpOptions = smtpOptions.Value;
        }

        // Shows the current user's reservations (all types)
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var model = new UserReservationsViewModel
            {
                Lockers = await _context.LockerReservations
                    .Where(l => l.UserId == user.Id)
                    .OrderByDescending(l => l.RequestDate)
                    .ToListAsync(),

                Activities = await _context.ActivityReservations
                    .Where(a => a.UserId == user.Id)
                    .OrderByDescending(a => a.RequestDate)
                    .ToListAsync(),

                GatePasses = await _context.GatePasses
                    .Where(g => g.UserId == user.Id)
                    .OrderByDescending(g => g.RequestDate)
                    .ToListAsync()
            };

            return View(model);
        }

        // ---------------- LOCKER ----------------
        [HttpGet]
        public IActionResult CreateLocker()
        {
            return View(new CreateLockerViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLocker(CreateLockerViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var reservation = new LockerReservationModel
            {
                LockerNumber = model.LockerNumber,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                Purpose = model.Purpose,
                Status = "Pending",
                RequestDate = DateTime.Now,
                UserId = user.Id,
                UserEmail = user.Email
            };

            _context.LockerReservations.Add(reservation);
            await _context.SaveChangesAsync();

            // Notify admin
            var adminEmail = _smtpOptions.AdminEmail ?? "administrator123@admin.com";
            var subject = $"New Locker Reservation from {user.Email}";
            var body = $@"
                <p>User <strong>{WebUtility.HtmlEncode(user.Email)}</strong> requested locker <strong>{WebUtility.HtmlEncode(reservation.LockerNumber)}</strong>.</p>
                <p>Period: {reservation.StartDate:yyyy-MM-dd} — {reservation.EndDate:yyyy-MM-dd}</p>
                <p>Purpose: {WebUtility.HtmlEncode(reservation.Purpose)}</p>
                <p><a href=""{Url.Action("LockerReservations", "Admin", null, Request.Scheme)}"">View in Admin Panel</a></p>
            ";
            try { await _emailSender.SendEmailAsync(adminEmail, subject, body); } catch { /* swallow for now */ }

            TempData["Success"] = "Locker reservation request submitted.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelLocker(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var reservation = await _context.LockerReservations.FindAsync(id);
            if (reservation == null || reservation.UserId != user.Id)
                return NotFound();

            _context.LockerReservations.Remove(reservation);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Locker reservation cancelled.";
            return RedirectToAction(nameof(Index));
        }

        // ---------------- ACTIVITY ----------------
        [HttpGet]
        public IActionResult CreateActivity()
        {
            return View(new CreateActivityViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateActivity(CreateActivityViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var reservation = new ActivityReservationModel
            {
                ActivityName = model.ActivityName,
                ActivityDate = model.ActivityDate,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                Description = model.Description,
                Status = "Pending",
                RequestDate = DateTime.Now,
                UserId = user.Id,
                UserEmail = user.Email
            };

            _context.ActivityReservations.Add(reservation);
            await _context.SaveChangesAsync();

            // Notify admin
            var adminEmail = _smtpOptions.AdminEmail ?? "administrator123@admin.com";
            var subject = $"New Activity Reservation from {user.Email}";
            var body = $@"
                <p>User <strong>{WebUtility.HtmlEncode(user.Email)}</strong> requested activity <strong>{WebUtility.HtmlEncode(reservation.ActivityName)}</strong> on {reservation.ActivityDate:yyyy-MM-dd}.</p>
                <p>Time: {reservation.StartTime:hh\\:mm} — {reservation.EndTime:hh\\:mm}</p>
                <p>Description: {WebUtility.HtmlEncode(reservation.Description)}</p>
                <p><a href=""{Url.Action("ActivityReservations", "Admin", null, Request.Scheme)}"">View in Admin Panel</a></p>
            ";
            try { await _emailSender.SendEmailAsync(adminEmail, subject, body); } catch { }

            TempData["Success"] = "Activity reservation request submitted.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelActivity(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var reservation = await _context.ActivityReservations.FindAsync(id);
            if (reservation == null || reservation.UserId != user.Id)
                return NotFound();

            _context.ActivityReservations.Remove(reservation);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Activity reservation cancelled.";
            return RedirectToAction(nameof(Index));
        }

        // ---------------- GATE PASS ----------------
        [HttpGet]
        public IActionResult CreateGatePass()
        {
            return View(new CreateGatePassViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGatePass(CreateGatePassViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var pass = new GatePass
            {
                Destination = model.Destination,
                ExitDate = model.ExitDate,
                ExitTime = model.ExitTime,
                ReturnDate = model.ReturnDate,
                ReturnTime = model.ReturnTime,
                Reason = model.Reason,
                Status = "Pending",
                RequestDate = DateTime.Now,
                UserId = user.Id,
                UserEmail = user.Email
            };

            _context.GatePasses.Add(pass);
            await _context.SaveChangesAsync();

            // Notify admin
            var adminEmail = _smtpOptions.AdminEmail ?? "administrator123@admin.com";
            var subject = $"New Gate Pass Request from {user.Email}";
            var body = $@"
                <p>User <strong>{WebUtility.HtmlEncode(user.Email)}</strong> requested a gate pass to <strong>{WebUtility.HtmlEncode(pass.Destination)}</strong>.</p>
                <p>Exit: {pass.ExitDate:yyyy-MM-dd} {pass.ExitTime:hh\\:mm}</p>
                <p>Return: {(pass.ReturnDate.HasValue ? pass.ReturnDate.Value.ToString("yyyy-MM-dd") : "-")} {(pass.ReturnTime.HasValue ? pass.ReturnTime.Value.ToString(@"hh\:mm") : "")}</p>
                <p>Reason: {WebUtility.HtmlEncode(pass.Reason)}</p>
                <p><a href=""{Url.Action("GatePasses", "Admin", null, Request.Scheme)}"">View in Admin Panel</a></p>
            ";
            try { await _emailSender.SendEmailAsync(adminEmail, subject, body); } catch { }

            TempData["Success"] = "Gate pass request submitted.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelGatePass(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var pass = await _context.GatePasses.FindAsync(id);
            if (pass == null || pass.UserId != user.Id)
                return NotFound();

            _context.GatePasses.Remove(pass);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Gate pass request cancelled.";
            return RedirectToAction(nameof(Index));
        }
    }
}