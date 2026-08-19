using Microsoft.AspNetCore.Identity;

namespace HabitTrackerAspNetMVCWebApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        public bool IsActive { get; set; } = true;
    }
}