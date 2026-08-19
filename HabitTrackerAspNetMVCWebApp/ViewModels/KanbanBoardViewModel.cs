using System;

using HabitTrackerAspNetMVCWebApp.Models;

namespace HabitTrackerAspNetMVCWebApp.ViewModels
{
    public class KanbanBoardViewModel
    {
        public List<Habit> TodoHabits { get; set; } = new();
        public List<Habit> InProgressHabits { get; set; } = new();
        public List<Habit> DoneHabits { get; set; } = new();
    }
}
