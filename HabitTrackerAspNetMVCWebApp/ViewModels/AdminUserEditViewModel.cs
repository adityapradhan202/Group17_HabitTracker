using System.ComponentModel.DataAnnotations;

namespace HabitTrackerAspNetMVCWebApp.ViewModels
{
    public class AdminUserEditViewModel
    {
        [Required]
        public string Id { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Role")]
        public string RoleName { get; set; } = "User";

        [Display(Name = "Active")]
        public bool IsActive { get; set; }
    }
}