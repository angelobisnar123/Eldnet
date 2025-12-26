using System;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1_Test1.Models
{
    public class GatePassViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CollegeDepartment { get; set; }
        public string Course { get; set; }
        public string YearLevel { get; set; }
        public string SchoolYear { get; set; }
        public string ClassType { get; set; }
        public string Address { get; set; }
        public string PlateNumber { get; set; }
        public string VehicleType { get; set; }
        public string Color { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public DateTime DateToday { get; set; }
        public DateTime RegistrationExpiryDate { get; set; }

        // This property is used in your Index view
        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}
