using System.Collections.Generic;
using HabitTracker.Api.Models;

namespace HabitTracker.Api.ViewModels
{
    public class KanbanBoardViewModel
    {
        public List<Habit> TodoHabits { get; set; } = new List<Habit>();
        public List<Habit> InProgressHabits { get; set; } = new List<Habit>();
        public List<Habit> DoneHabits { get; set; } = new List<Habit>();
    }
}
