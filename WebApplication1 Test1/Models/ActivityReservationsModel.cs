using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1_Test1.Models
{
    [Table("ActivityReservations")]
    public class ActivityReservationModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [Display(Name = "Activity Name")]
        [MaxLength(200)]
        public string ActivityName { get; set; }

        [Required]
        [Display(Name = "Activity Date")]
        [DataType(DataType.Date)]
        public DateTime ActivityDate { get; set; }

        [Required]
        [Display(Name = "Start Time")]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [Required]
        [Display(Name = "End Time")]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }

        [Required]
        [Display(Name = "Description")]
        [MaxLength(500)]
        public string Description { get; set; }

        [Required]
        public string Status { get; set; } = "Pending";

        [Required]
        [Display(Name = "Request Date")]
        public DateTime RequestDate { get; set; } = DateTime.Now;

        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string UserEmail { get; set; }
    }
}