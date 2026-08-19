using HabitTrackerAspNetMVCWebApp.Models;

namespace HabitTrackerAspNetMVCWebApp.Services
{
    public class HabitScheduleService
    {
        public bool IsHabitPlannedForDate(Habit habit, DateTime date)
        {
            var targetDate = date.Date;

            if (targetDate < habit.StartDate.Date)
                return false;

            if (habit.EndDate.HasValue && targetDate > habit.EndDate.Value.Date)
                return false;

            if (habit.Status == HabitStatus.Paused)
                return false;

            return habit.Frequency switch
            {
                Frequency.Daily => true,
                Frequency.Weekly => habit.StartDate.DayOfWeek == targetDate.DayOfWeek,
                Frequency.Monthly => habit.StartDate.Day == targetDate.Day,
                _ => false
            };
        }
    }
}