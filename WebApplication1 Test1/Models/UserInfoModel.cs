using System.ComponentModel.DataAnnotations;

namespace WebApplication1_Test1.Models
{
    public class UserInfoModel
    {
        [Key]
        public string UserId { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [Display(Name = "Last Name")]
        [MaxLength(100)]
        public string LastName { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [Display(Name = "First Name")]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Contact number is required")]
        [Display(Name = "Contact Number")]
        [MaxLength(20)]
        public string ContactNumber { get; set; }

        [Required(ErrorMessage = "Semester is required")]
        [MaxLength(50)]
        public string Semester { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email Address")]
        public string Email { get; set; }
    }

    public class UserInfoViewModel
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string ContactNumber { get; set; }
        public string Semester { get; set; }
        public bool IsRegistered { get; set; }
    }
}