using Microsoft.AspNetCore.Identity;

namespace HabitTracker.Api.Models
{
    public class ApplicationUser : IdentityUser
    {
        public bool IsActive { get; set; } = true;
    }
}
