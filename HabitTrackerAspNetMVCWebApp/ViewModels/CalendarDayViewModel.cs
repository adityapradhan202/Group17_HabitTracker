using System;
using System.Collections.Generic;

using System;
using System.Collections.Generic;

namespace HabitTrackerAspNetMVCWebApp.ViewModels
{
    public class CalendarDayViewModel
    {
        public DateTime Date { get; set; }

        public bool IsCurrentMonth { get; set; }

        public bool IsToday { get; set; }

        public List<CalendarHabitItemViewModel> PlannedHabits { get; set; } = new();

        public List<CalendarHabitItemViewModel> PartialHabits { get; set; } = new();

        public List<CalendarHabitItemViewModel> CompletedHabits { get; set; } = new();

        public List<CalendarHabitItemViewModel> SkippedHabits { get; set; } = new();
    }
}