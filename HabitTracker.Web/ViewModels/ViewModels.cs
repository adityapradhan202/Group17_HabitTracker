using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HabitTracker.Api.Models;

namespace HabitTracker.Web.ViewModels
{
    public class CalendarHabitItemViewModel
    {
        public int HabitId { get; set; }
        public string Title { get; set; } = string.Empty;
        public HabitLogStatus? CurrentStatus { get; set; }
    }

    public class CalendarDayViewModel
    {
        public DateTime Date { get; set; }
        public bool IsCurrentMonth { get; set; }
        public bool IsToday { get; set; }

        public List<CalendarHabitItemViewModel> PlannedHabits { get; set; } = new List<CalendarHabitItemViewModel>();
        public List<CalendarHabitItemViewModel> PartialHabits { get; set; } = new List<CalendarHabitItemViewModel>();
        public List<CalendarHabitItemViewModel> CompletedHabits { get; set; } = new List<CalendarHabitItemViewModel>();
        public List<CalendarHabitItemViewModel> SkippedHabits { get; set; } = new List<CalendarHabitItemViewModel>();
    }

    public class CalendarViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;

        public List<CalendarDayViewModel> Days { get; set; } = new List<CalendarDayViewModel>();

        public int PrevMonth { get; set; }
        public int PrevYear { get; set; }
        public int NextMonth { get; set; }
        public int NextYear { get; set; }
    }

    public class KanbanBoardViewModel
    {
        public List<Habit> TodoHabits { get; set; } = new List<Habit>();
        public List<Habit> InProgressHabits { get; set; } = new List<Habit>();
        public List<Habit> DoneHabits { get; set; } = new List<Habit>();
    }

    public class AdminUserListItemViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int HabitCount { get; set; }
        public bool IsActive { get; set; } = true;
        public string RoleName { get; set; } = "User";
    }

    public class AdminUserCreateViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Role")]
        public string RoleName { get; set; } = "User";

        [Display(Name = "Active Account")]
        public bool IsActive { get; set; } = true;
    }

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
