using System.Collections.Generic;

namespace HabitTrackerAspNetMVCWebApp.ViewModels
{
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
}