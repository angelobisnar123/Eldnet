using System.ComponentModel.DataAnnotations;

namespace WebApplication1_Test1.Models
{
    public class UserManagementViewModel
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public bool IsLockedOut { get; set; }
        public bool IsBanned { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime RegisteredDate { get; set; }
    }

    public class LockerReservation
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string LockerNumber { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [StringLength(500)]
        public string Purpose { get; set; }

        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

        public DateTime RequestDate { get; set; } = DateTime.Now;

        public string UserEmail { get; set; }
        public string IdNumber { get; internal set; }
    }

    public class ActivityReservation
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [StringLength(200)]
        public string ActivityName { get; set; }

        [Required]
        public DateTime ActivityDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public string Status { get; set; } = "Pending";

        public DateTime RequestDate { get; set; } = DateTime.Now;

        public string UserEmail { get; set; }
    }

    public class GatePass
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [StringLength(200)]
        public string Destination { get; set; }

        [Required]
        public DateTime ExitDate { get; set; }

        [Required]
        public TimeSpan ExitTime { get; set; }

        public DateTime? ReturnDate { get; set; }

        public TimeSpan? ReturnTime { get; set; }

        [StringLength(500)]
        public string Reason { get; set; }

        public string Status { get; set; } = "Pending";

        public DateTime RequestDate { get; set; } = DateTime.Now;

        public string UserEmail { get; set; }
    }
}