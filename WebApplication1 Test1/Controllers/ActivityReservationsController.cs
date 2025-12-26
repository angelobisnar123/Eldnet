using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1_Test1.Data;
using WebApplication1_Test1.Models;

namespace WebApplication1_Test1.Controllers
{
    public class ActivityReservationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ActivityReservationsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: ActivityReservations
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            var reservations = await _context.ActivityReservations
                .Where(r => r.UserId == currentUser.Id)
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();

            return View(reservations);
        }

        // GET: ActivityReservations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var activityReservation = await _context.ActivityReservations
                .FirstOrDefaultAsync(m => m.Id == id);
            if (activityReservation == null)
            {
                return NotFound();
            }

            return View(activityReservation);
        }

        // GET: ActivityReservations/Create
        public IActionResult Create()
        {
            var currentUser = _userManager.GetUserAsync(User).Result;

            var model = new ActivityReservationModel
            {
                ActivityDate = DateTime.Today,
                StartTime = new TimeSpan(9, 0, 0), // 9:00 AM
                EndTime = new TimeSpan(17, 0, 0),  // 5:00 PM
                Status = "Pending",
                RequestDate = DateTime.Now,
                UserEmail = currentUser?.Email ?? ""
            };
            return View(model);
        }

        // POST: ActivityReservations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ActivityReservationModel model)
        {
            Console.WriteLine($"Create POST method called");
            Console.WriteLine($"Model state is valid: {ModelState.IsValid}");

            if (!ModelState.IsValid)
            {
                Console.WriteLine($"ModelState errors:");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"  - {error.ErrorMessage}");
                }

                // Try to get current user info for debugging
                var currentUser = await _userManager.GetUserAsync(User);
                Console.WriteLine($"Current user: {currentUser?.Id} - {currentUser?.Email}");
                Console.WriteLine($"Model UserId: {model.UserId}");
                Console.WriteLine($"Model UserEmail: {model.UserEmail}");

                return View(model);
            }

            try
            {
                // Get current logged-in user
                var currentUser = await _userManager.GetUserAsync(User);
                Console.WriteLine($"Current user retrieved: {currentUser?.Id} - {currentUser?.Email}");

                if (currentUser == null)
                {
                    Console.WriteLine("Current user is null, returning Challenge");
                    return Challenge();
                }

                // Set UserId to the logged-in user's ID
                model.UserId = currentUser.Id;
                model.RequestDate = DateTime.Now;
                model.Status = "Pending";

                Console.WriteLine($"Setting UserId: {model.UserId}");
                Console.WriteLine($"Activity Name: {model.ActivityName}");
                Console.WriteLine($"Activity Date: {model.ActivityDate}");
                Console.WriteLine($"Start Time: {model.StartTime}");
                Console.WriteLine($"End Time: {model.EndTime}");
                Console.WriteLine($"Description length: {model.Description?.Length}");
                Console.WriteLine($"UserEmail: {model.UserEmail}");

                // Check for overlapping reservations at the same time
                var existingReservation = await _context.ActivityReservations
                    .Where(r => r.Status == "Approved" || r.Status == "Pending")
                    .Where(r => r.ActivityDate == model.ActivityDate)
                    .Where(r => (model.StartTime < r.EndTime && model.EndTime > r.StartTime))
                    .FirstOrDefaultAsync();

                if (existingReservation != null)
                {
                    Console.WriteLine($"Found overlapping reservation: {existingReservation.Id}");
                    ModelState.AddModelError("ActivityDate",
                        $"There is already an activity scheduled on {model.ActivityDate:MM/dd/yyyy} from {existingReservation.StartTime:hh\\:mm} to {existingReservation.EndTime:hh\\:mm}");
                    return View(model);
                }

                Console.WriteLine("Adding model to context...");
                _context.ActivityReservations.Add(model);

                Console.WriteLine("Saving changes to database...");
                await _context.SaveChangesAsync();

                Console.WriteLine($"Activity reservation #{model.Id} created successfully!");
                TempData["SuccessMessage"] = $"Activity reservation #{model.Id} created successfully! Status: {model.Status}";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"Database error: {dbEx.Message}");
                Console.WriteLine($"Inner exception: {dbEx.InnerException?.Message}");
                ModelState.AddModelError("", $"Database error: {dbEx.InnerException?.Message ?? dbEx.Message}");
                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                return View(model);
            }
        }

        // GET: ActivityReservations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var activityReservation = await _context.ActivityReservations.FindAsync(id);
            if (activityReservation == null)
            {
                return NotFound();
            }

            return View(activityReservation);
        }

        // POST: ActivityReservations/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ActivityReservationModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Preserve existing UserId and RequestDate
                var existingReservation = await _context.ActivityReservations.FindAsync(id);
                if (existingReservation != null)
                {
                    model.UserId = existingReservation.UserId;
                    model.RequestDate = existingReservation.RequestDate;
                }

                _context.Update(model);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Activity reservation updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ActivityReservationExists(model.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (DbUpdateException dbEx)
            {
                ModelState.AddModelError("", $"Database error: {dbEx.InnerException?.Message ?? dbEx.Message}");
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                return View(model);
            }
        }

        // GET: ActivityReservations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var activityReservation = await _context.ActivityReservations
                .FirstOrDefaultAsync(m => m.Id == id);
            if (activityReservation == null)
            {
                return NotFound();
            }

            return View(activityReservation);
        }

        // POST: ActivityReservations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var activityReservation = await _context.ActivityReservations.FindAsync(id);
                if (activityReservation != null)
                {
                    _context.ActivityReservations.Remove(activityReservation);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Activity reservation deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Activity reservation not found!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting activity reservation: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ActivityReservationExists(int id)
        {
            return _context.ActivityReservations.Any(e => e.Id == id);
        }
    }
}