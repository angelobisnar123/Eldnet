using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1_Test1.Data;
using WebApplication1_Test1.Models;

namespace WebApplication1_Test1.Controllers
{
    public class LockerReservationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public LockerReservationsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: LockerReservations
        public async Task<IActionResult> Index()
        {
            // Get current logged-in user
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge(); // Redirect to login if not authenticated
            }

            // Get current user's reservations only
            var reservations = await _context.LockerReservations
                .Where(r => r.UserId == currentUser.Id)
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();

            return View(reservations);
        }

        // GET: LockerReservations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lockerReservation = await _context.LockerReservations
                .FirstOrDefaultAsync(m => m.Id == id);
            if (lockerReservation == null)
            {
                return NotFound();
            }

            return View(lockerReservation);
        }

        // GET: LockerReservations/Create
        public IActionResult Create()
        {
            var currentUser = _userManager.GetUserAsync(User).Result;

            var model = new LockerReservationModel
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddYears(1),
                RequestDate = DateTime.Now,
                Status = "Pending",
                UserEmail = currentUser?.Email ?? ""
            };
            return View(model);
        }

        // POST: LockerReservations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("UserId,LockerNumber,StartDate,EndDate,Purpose,Status,RequestDate,UserEmail,LastName,FirstName,ContactNumber,Semester")]
            LockerReservationModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Get current logged-in user
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Challenge();
                }

                // Set UserId to the logged-in user's ID
                model.UserId = currentUser.Id;

                // Combine personal information and store in Purpose field
                var personalInfo = $"Name: {model.FirstName} {model.LastName}, Contact: {model.ContactNumber}, Semester: {model.Semester}";

                // Append to existing purpose if any
                model.Purpose = string.IsNullOrEmpty(model.Purpose)
                    ? "No Purpose Provided. "
                    : $"{model.Purpose}";

                // Set default values
                model.RequestDate = DateTime.Now;
                model.Status = "Pending";

                // Check if locker is already reserved for overlapping dates
                var existingReservation = await _context.LockerReservations
                    .Where(r => r.LockerNumber == model.LockerNumber)
                    .Where(r => r.Status != "Cancelled" && r.Status != "Rejected")
                    .Where(r => (model.StartDate <= r.EndDate && model.EndDate >= r.StartDate))
                    .FirstOrDefaultAsync();

                if (existingReservation != null)
                {
                    ModelState.AddModelError("LockerNumber",
                        $"Locker {model.LockerNumber} is already reserved from {existingReservation.StartDate:MM/dd/yyyy} to {existingReservation.EndDate:MM/dd/yyyy}");
                    return View(model);
                }

                _context.LockerReservations.Add(model);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Reservation #{model.Id} created successfully! Status: {model.Status}";
                return RedirectToAction(nameof(Index));
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

        // GET: LockerReservations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lockerReservation = await _context.LockerReservations.FindAsync(id);
            if (lockerReservation == null)
            {
                return NotFound();
            }

            // Extract personal information from Purpose field to display in form
            var personalInfo = ParsePersonalInfo(lockerReservation.Purpose);
            ViewBag.LastName = personalInfo.LastName;
            ViewBag.FirstName = personalInfo.FirstName;
            ViewBag.ContactNumber = personalInfo.ContactNumber;
            ViewBag.Semester = personalInfo.Semester;

            return View(lockerReservation);
        }

        // POST: LockerReservations/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("Id,UserId,LockerNumber,StartDate,EndDate,Purpose,Status,RequestDate,UserEmail")]
            LockerReservationModel model,
            string LastName,
            string FirstName,
            string ContactNumber,
            string Semester)
        {
            // Return form data to ViewBag for repopulation
            ViewBag.LastName = LastName;
            ViewBag.FirstName = FirstName;
            ViewBag.ContactNumber = ContactNumber;
            ViewBag.Semester = Semester;

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
                // Extract existing purpose (excluding personal info if it exists)
                var originalPurpose = ExtractOriginalPurpose(model.Purpose);

                // Combine personal information and store in Purpose field
                var personalInfo = $"Name: {FirstName} {LastName}, Contact: {ContactNumber}, Semester: {Semester}";

                // Combine original purpose with personal info
                model.Purpose = string.IsNullOrEmpty(originalPurpose)
                    ? personalInfo
                    : $"{originalPurpose}. {personalInfo}";

                _context.Update(model);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Reservation updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LockerReservationExists(model.Id))
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

        // GET: LockerReservations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lockerReservation = await _context.LockerReservations
                .FirstOrDefaultAsync(m => m.Id == id);
            if (lockerReservation == null)
            {
                return NotFound();
            }

            return View(lockerReservation);
        }

        // POST: LockerReservations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var lockerReservation = await _context.LockerReservations.FindAsync(id);
                if (lockerReservation != null)
                {
                    _context.LockerReservations.Remove(lockerReservation);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Reservation deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Reservation not found!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting reservation: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: LockerReservations/UpdateStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            try
            {
                var lockerReservation = await _context.LockerReservations.FindAsync(id);
                if (lockerReservation == null)
                {
                    TempData["ErrorMessage"] = "Reservation not found!";
                    return RedirectToAction(nameof(Index));
                }

                lockerReservation.Status = status;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Reservation #{id} status updated to {status}";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating status: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool LockerReservationExists(int id)
        {
            return _context.LockerReservations.Any(e => e.Id == id);
        }

        // Helper method to parse personal information from Purpose field - FIXED
        private (string LastName, string FirstName, string ContactNumber, string Semester)
            ParsePersonalInfo(string purpose)
        {
            string lastName = "";
            string firstName = "";
            string contactNumber = "";
            string semester = "";

            if (!string.IsNullOrEmpty(purpose))
            {
                // Try to parse the pattern we stored
                try
                {
                    var namePart = purpose.Split(new[] { "Name:" }, StringSplitOptions.RemoveEmptyEntries);
                    if (namePart.Length > 1)
                    {
                        var name = namePart[1].Split(',')[0].Trim();
                        var nameParts = name.Split(' ');
                        if (nameParts.Length >= 2)
                        {
                            firstName = nameParts[0];
                            lastName = nameParts[1];
                        }
                        else if (nameParts.Length == 1)
                        {
                            firstName = nameParts[0];
                        }
                    }

                    var contactPart = purpose.Split(new[] { "Contact:" }, StringSplitOptions.RemoveEmptyEntries);
                    if (contactPart.Length > 1)
                    {
                        contactNumber = contactPart[1].Split(',')[0].Trim();
                    }

                    var semesterPart = purpose.Split(new[] { "Semester:" }, StringSplitOptions.RemoveEmptyEntries);
                    if (semesterPart.Length > 1)
                    {
                        semester = semesterPart[1].Trim();
                        // Remove any trailing period
                        if (semester.EndsWith("."))
                        {
                            semester = semester.Substring(0, semester.Length - 1);
                        }
                    }
                }
                catch
                {
                    // If parsing fails, return empty values
                }
            }

            return (lastName, firstName, contactNumber, semester);
        }

        // Helper method to extract original purpose (without personal info)
        private string ExtractOriginalPurpose(string purpose)
        {
            if (string.IsNullOrEmpty(purpose))
                return "";

            // Check if purpose contains our personal info pattern
            if (purpose.Contains("Name:") && purpose.Contains("Contact:") && purpose.Contains("Semester:"))
            {
                // Find the position of "Name:" and extract everything before it
                var nameIndex = purpose.IndexOf("Name:");
                if (nameIndex > 0)
                {
                    var original = purpose.Substring(0, nameIndex).Trim();
                    // Remove trailing period if exists
                    if (original.EndsWith("."))
                    {
                        original = original.Substring(0, original.Length - 1);
                    }
                    return original;
                }
            }

            return purpose;
        }
    }
}