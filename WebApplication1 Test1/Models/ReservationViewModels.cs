using System.ComponentModel.DataAnnotations;

namespace WebApplication1_Test1.Models
{
    // View models used by the user-facing reservation forms

    public class CreateLockerViewModel
    {
        [Required]
        [StringLength(50)]
        [Display(Name = "Locker Number")]
        public string LockerNumber { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        [StringLength(500)]
        public string Purpose { get; set; }
    }

    public class CreateActivityViewModel
    {
        [Required]
        [StringLength(200)]
        [Display(Name = "Activity Name")]
        public string ActivityName { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Activity Date")]
        public DateTime ActivityDate { get; set; }

        [Required]
        [DataType(DataType.Time)]
        [Display(Name = "Start Time")]
        public TimeSpan StartTime { get; set; }

        [Required]
        [DataType(DataType.Time)]
        [Display(Name = "End Time")]
        public TimeSpan EndTime { get; set; }

        [StringLength(500)]
        public string Description { get; set; }
    }

    public class CreateGatePassViewModel
    {
        [Required]
        [StringLength(200)]
        public string Destination { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Exit Date")]
        public DateTime ExitDate { get; set; }

        [Required]
        [DataType(DataType.Time)]
        [Display(Name = "Exit Time")]
        public TimeSpan ExitTime { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Return Date (optional)")]
        public DateTime? ReturnDate { get; set; }

        [DataType(DataType.Time)]
        [Display(Name = "Return Time (optional)")]
        public TimeSpan? ReturnTime { get; set; }

        [StringLength(500)]
        public string Reason { get; set; }
    }

    // Combined model for the Index view
    public class UserReservationsViewModel
    {
        public List<LockerReservationModel> Lockers { get; set; } = new();
        public List<ActivityReservationModel> Activities { get; set; } = new();
        public List<GatePass> GatePasses { get; set; } = new();
    }
}