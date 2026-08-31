using System;
using System.Collections.Generic;

namespace HabitTracker.Api.ViewModels
{
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
}
