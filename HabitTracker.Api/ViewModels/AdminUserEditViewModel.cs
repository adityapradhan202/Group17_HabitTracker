using System.ComponentModel.DataAnnotations;

namespace HabitTracker.Api.ViewModels
{
    public class AdminUserEditViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Role")]
        public string RoleName { get; set; } = "User";

        [Display(Name = "Active Account")]
        public bool IsActive { get; set; } = true;
    }
}
