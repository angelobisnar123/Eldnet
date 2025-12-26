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
    [Authorize] // Only logged-in users
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly SmtpOptions _smtpOptions;

        public AdminController(UserManager<IdentityUser> userManager, ApplicationDbContext context, IEmailSender emailSender, IOptions<SmtpOptions> smtpOptions)
        {
            _userManager = userManager;
            _context = context;
            _emailSender = emailSender;
            _smtpOptions = smtpOptions.Value;
        }

        // Check if user is admin
        private async Task<bool> IsAdmin()
        {
            var user = await _userManager.GetUserAsync(User);
            return user?.Email == "administrator123@admin.com";
        }

        // Admin Dashboard
        public async Task<IActionResult> Index()
        {
            if (!await IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            var users = await _userManager.Users.ToListAsync();
            var userViewModels = users.Select(u => new UserManagementViewModel
            {
                UserId = u.Id,
                Email = u.Email,
                LockoutEnd = u.LockoutEnd?.DateTime,
                IsLockedOut = u.LockoutEnd.HasValue && u.LockoutEnd > DateTimeOffset.Now,
                IsBanned = u.LockoutEnd.HasValue && u.LockoutEnd > DateTimeOffset.Now.AddYears(10)
            }).ToList();

            return View(userViewModels);
        }

        // ... user management methods unchanged ...

        // === LOCKER RESERVATIONS ===
        public async Task<IActionResult> LockerReservations()
        {
            if (!await IsAdmin())
                return RedirectToAction("Index", "Home");

            var reservations = await _context.LockerReservations
                .OrderByDescending(l => l.RequestDate)
                .ToListAsync();

            // Get user emails
            foreach (var reservation in reservations)
            {
                var user = await _userManager.FindByIdAsync(reservation.UserId);
                reservation.UserEmail = user?.Email ?? "Unknown";
            }

            return View(reservations);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveLocker(int id)
        {
            if (!await IsAdmin())
                return RedirectToAction("Index", "Home");

            var reservation = await _context.LockerReservations.FindAsync(id);
            if (reservation == null)
                return NotFound();

            reservation.Status = "Approved";
            await _context.SaveChangesAsync();

            // Notify user
            var user = await _userManager.FindByIdAsync(reservation.UserId);
            if (user != null)
            {
                var subject = "Your locker reservation has been approved";
                var body = $@"
                    <p>Hello {WebUtility.HtmlEncode(user.Email)},</p>
                    <p>Your locker reservation (<strong>{WebUtility.HtmlEncode(reservation.LockerNumber)}</strong>) from {reservation.StartDate:yyyy-MM-dd} to {reservation.EndDate:yyyy-MM-dd} has been <strong>approved</strong>.</p>
                    <p>Thank you.</p>
                ";
                try { await _emailSender.SendEmailAsync(user.Email, subject, body); } catch { }
            }

            TempData["Success"] = "Locker reservation approved.";
            return RedirectToAction(nameof(LockerReservations));
        }

        [HttpPost]
        public async Task<IActionResult> RejectLocker(int id)
        {
            if (!await IsAdmin())
                return RedirectToAction("Index", "Home");

            var reservation = await _context.LockerReservations.FindAsync(id);
            if (reservation == null)
                return NotFound();

            reservation.Status = "Rejected";
            await _context.SaveChangesAsync();

            var user = await _userManager.FindByIdAsync(reservation.UserId);
            if (user != null)
            {
                var subject = "Your locker reservation has been rejected";
                var body = $@"
                    <p>Hello {WebUtility.HtmlEncode(user.Email)},</p>
                    <p>Your locker reservation (<strong>{WebUtility.HtmlEncode(reservation.LockerNumber)}</strong>) has been <strong>rejected</strong>. Please contact admin for details.</p>
                ";
                try { await _emailSender.SendEmailAsync(user.Email, subject, body); } catch { }
            }

            TempData["Success"] = "Locker reservation rejected.";
            return RedirectToAction(nameof(LockerReservations));
        }

        // === ACTIVITY RESERVATIONS ===
        public async Task<IActionResult> ActivityReservations()
        {
            if (!await IsAdmin())
                return RedirectToAction("Index", "Home");

            var reservations = await _context.ActivityReservations
                .OrderByDescending(a => a.RequestDate)
                .ToListAsync();

            foreach (var reservation in reservations)
            {
                var user = await _userManager.FindByIdAsync(reservation.UserId);
                reservation.UserEmail = user?.Email ?? "Unknown";
            }

            return View(reservations);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveActivity(int id)
        {
            if (!await IsAdmin())
                return RedirectToAction("Index", "Home");

            var reservation = await _context.ActivityReservations.FindAsync(id);
            if (reservation == null)
                return NotFound();

            reservation.Status = "Approved";
            await _context.SaveChangesAsync();

            var user = await _userManager.FindByIdAsync(reservation.UserId);
            if (user != null)
            {
                var subject = "Your activity reservation has been approved";
                var body = $@"
                    <p>Hello {WebUtility.HtmlEncode(user.Email)},</p>
                    <p>Your reservation for <strong>{WebUtility.HtmlEncode(reservation.ActivityName)}</strong> on {reservation.ActivityDate:yyyy-MM-dd} has been <strong>approved</strong>.</p>
                ";
                try { await _emailSender.SendEmailAsync(user.Email, subject, body); } catch { }
            }

            TempData["Success"] = "Activity reservation approved.";
            return RedirectToAction(nameof(ActivityReservations));
        }

        [HttpPost]
        public async Task<IActionResult> RejectActivity(int id)
        {
            if (!await IsAdmin())
                return RedirectToAction("Index", "Home");

            var reservation = await _context.ActivityReservations.FindAsync(id);
            if (reservation == null)
                return NotFound();

            reservation.Status = "Rejected";
            await _context.SaveChangesAsync();

            var user = await _userManager.FindByIdAsync(reservation.UserId);
            if (user != null)
            {
                var subject = "Your activity reservation has been rejected";
                var body = $@"
                    <p>Hello {WebUtility.HtmlEncode(user.Email)},</p>
                    <p>Your reservation for <strong>{WebUtility.HtmlEncode(reservation.ActivityName)}</strong> has been <strong>rejected</strong>. Please contact admin for details.</p>
                ";
                try { await _emailSender.SendEmailAsync(user.Email, subject, body); } catch { }
            }

            TempData["Success"] = "Activity reservation rejected.";
            return RedirectToAction(nameof(ActivityReservations));
        }

        // === GATE PASSES ===
        public async Task<IActionResult> GatePasses()
        {
            if (!await IsAdmin())
                return RedirectToAction("Index", "Home");

            var passes = await _context.GatePasses
                .OrderByDescending(g => g.RequestDate)
                .ToListAsync();

            foreach (var pass in passes)
            {
                var user = await _userManager.FindByIdAsync(pass.UserId);
                pass.UserEmail = user?.Email ?? "Unknown";
            }

            return View(passes);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveGatePass(int id)
        {
            if (!await IsAdmin())
                return RedirectToAction("Index", "Home");

            var pass = await _context.GatePasses.FindAsync(id);
            if (pass == null)
                return NotFound();

            pass.Status = "Approved";
            await _context.SaveChangesAsync();

            var user = await _userManager.FindByIdAsync(pass.UserId);
            if (user != null)
            {
                var subject = "Your gate pass has been approved";
                var body = $@"
                    <p>Hello {WebUtility.HtmlEncode(user.Email)},</p>
                    <p>Your gate pass to <strong>{WebUtility.HtmlEncode(pass.Destination)}</strong> has been <strong>approved</strong>.</p>
                ";
                try { await _emailSender.SendEmailAsync(user.Email, subject, body); } catch { }
            }

            TempData["Success"] = "Gate pass approved.";
            return RedirectToAction(nameof(GatePasses));
        }

        [HttpPost]
        public async Task<IActionResult> RejectGatePass(int id)
        {
            if (!await IsAdmin())
                return RedirectToAction("Index", "Home");

            var pass = await _context.GatePasses.FindAsync(id);
            if (pass == null)
                return NotFound();

            pass.Status = "Rejected";
            await _context.SaveChangesAsync();

            var user = await _userManager.FindByIdAsync(pass.UserId);
            if (user != null)
            {
                var subject = "Your gate pass has been rejected";
                var body = $@"
                    <p>Hello {WebUtility.HtmlEncode(user.Email)},</p>
                    <p>Your gate pass request to <strong>{WebUtility.HtmlEncode(pass.Destination)}</strong> has been <strong>rejected</strong>. Please contact admin for details.</p>
                ";
                try { await _emailSender.SendEmailAsync(user.Email, subject, body); } catch { }
            }

            TempData["Success"] = "Gate pass rejected.";
            return RedirectToAction(nameof(GatePasses));
        }
        [HttpPost]
        public async Task<IActionResult> DeleteLocker(int id)
        {
            if (!await IsAdmin())
                return RedirectToAction("Index", "Home");

            var reservation = await _context.LockerReservations.FindAsync(id);
            if (reservation == null)
                return NotFound();

            _context.LockerReservations.Remove(reservation);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Locker reservation deleted.";
            return RedirectToAction(nameof(LockerReservations));
        }
        [HttpPost]
        public async Task<IActionResult> DeleteActivity(int id)
        {
            if (!await IsAdmin())
                return RedirectToAction("Index", "Home");

            var reservation = await _context.ActivityReservations.FindAsync(id);
            if (reservation == null)
                return NotFound();

            _context.ActivityReservations.Remove(reservation);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Activity reservation deleted.";
            return RedirectToAction(nameof(ActivityReservations));
        }
    }
}