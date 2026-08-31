using HabitTracker.Api.Models;

namespace HabitTracker.Api.ViewModels
{
    public class CalendarHabitItemViewModel
    {
        public int HabitId { get; set; }
        public string Title { get; set; } = string.Empty;
        public HabitLogStatus? CurrentStatus { get; set; }
    }
}
