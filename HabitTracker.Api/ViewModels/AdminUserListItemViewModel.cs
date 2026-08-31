namespace HabitTracker.Api.ViewModels
{
    public class AdminUserListItemViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int HabitCount { get; set; }
        public bool IsActive { get; set; } = true;
        public string RoleName { get; set; } = "User";
    }
}
