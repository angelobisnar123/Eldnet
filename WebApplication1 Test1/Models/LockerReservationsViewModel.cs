using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1_Test1.Models
{
    [Table("LockerReservations")]
    public class LockerReservationModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "User ID is required")]
        [Display(Name = "User ID")]
        public string UserId { get; set; } // Still no [Required] to handle programmatic assignment

        [Required(ErrorMessage = "Locker number is required")]
        [Display(Name = "Locker Number")]
        [MaxLength(50)]
        public string LockerNumber { get; set; }

        [Required]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Purpose is required")]
        [MaxLength(500)]
        public string Purpose { get; set; }

        [Required]
        public string Status { get; set; } = "Pending";

        [Required]
        [Display(Name = "Request Date")]
        public DateTime RequestDate { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email Address")]
        public string UserEmail { get; set; }

        // These are NOT database columns, just form fields
        [NotMapped]
        [Required(ErrorMessage = "Last name is required")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [NotMapped]
        [Required(ErrorMessage = "First name is required")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [NotMapped]
        [Required(ErrorMessage = "Contact number is required")]
        [Display(Name = "Contact Number")]
        public string ContactNumber { get; set; }

        [NotMapped]
        [Required(ErrorMessage = "Semester is required")]
        public string Semester { get; set; }
    }
}