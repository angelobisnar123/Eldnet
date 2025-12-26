using Microsoft.AspNetCore.Mvc;
using WebApplication1_Test1.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication1_Test1.Controllers
{
    public class GatePassesController : Controller
    {
        private static List<GatePassViewModel> _gatePasses = new();

        public IActionResult Index()
        {
            return View(_gatePasses);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(GatePassViewModel model)
        {
            // DEBUG: Check what we're receiving
            System.Diagnostics.Debug.WriteLine("=== FORM SUBMISSION DEBUG ===");
            System.Diagnostics.Debug.WriteLine($"Model is null: {model == null}");

            if (model != null)
            {
                System.Diagnostics.Debug.WriteLine($"FirstName: '{model.FirstName ?? "NULL"}'");
                System.Diagnostics.Debug.WriteLine($"LastName: '{model.LastName ?? "NULL"}'");
                System.Diagnostics.Debug.WriteLine($"PlateNumber: '{model.PlateNumber ?? "NULL"}'");
                System.Diagnostics.Debug.WriteLine($"VehicleType: '{model.VehicleType ?? "NULL"}'");
                System.Diagnostics.Debug.WriteLine($"College: '{model.CollegeDepartment ?? "NULL"}'");
                System.Diagnostics.Debug.WriteLine($"Course: '{model.Course ?? "NULL"}'");

                model.DateToday = DateTime.Now;
                model.RegistrationExpiryDate = DateTime.Now.AddYears(1);
                _gatePasses.Add(model);

                System.Diagnostics.Debug.WriteLine($"Total records in list: {_gatePasses.Count}");
            }

            return RedirectToAction("Index");
        }

        public IActionResult Delete(string plateNumber)
        {
            var item = _gatePasses.FirstOrDefault(x => x.PlateNumber == plateNumber);
            if (item != null)
            {
                _gatePasses.Remove(item);
            }
            return RedirectToAction("Index");
        }

        public IActionResult ClearAll()
        {
            _gatePasses.Clear();
            return RedirectToAction("Index");
        }
    }
}