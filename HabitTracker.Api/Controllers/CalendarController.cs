using System.Security.Claims;
using HabitTracker.Api.Data;
using HabitTracker.Api.Models;
using HabitTracker.Api.Services;
using HabitTracker.Api.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "User")]
    public class CalendarController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly HabitScheduleService _scheduleService;

        public CalendarController(ApplicationDbContext context, HabitScheduleService scheduleService)
        {
            _context = context;
            _scheduleService = scheduleService;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }

        [HttpGet]
        public async Task<ActionResult<CalendarViewModel>> GetCalendar([FromQuery] int? year, [FromQuery] int? month)
        {
            var today = DateTime.Today;
            int selectedYear = year ?? today.Year;
            int selectedMonth = month ?? today.Month;

            var firstDayOfMonth = new DateTime(selectedYear, selectedMonth, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            var userId = GetCurrentUserId();

            var habits = await _context.Habits
                .Where(h => h.UserId == userId)
                .OrderBy(h => h.Title)
                .ToListAsync();

            var habitLogs = await _context.HabitLogs
                .Include(hl => hl.Habit)
                .Where(hl =>
                    hl.Habit != null &&
                    hl.Habit.UserId == userId &&
                    hl.LogDate.Date >= firstDayOfMonth.Date &&
                    hl.LogDate.Date <= lastDayOfMonth.Date)
                .ToListAsync();

            var days = BuildCalendarDays(firstDayOfMonth, lastDayOfMonth, habits, habitLogs);

            var viewModel = new CalendarViewModel
            {
                Year = selectedYear,
                Month = selectedMonth,
                MonthName = firstDayOfMonth.ToString("MMMM yyyy"),
                Days = days,
                PrevMonth = firstDayOfMonth.AddMonths(-1).Month,
                PrevYear = firstDayOfMonth.AddMonths(-1).Year,
                NextMonth = firstDayOfMonth.AddMonths(1).Month,
                NextYear = firstDayOfMonth.AddMonths(1).Year
            };

            return Ok(viewModel);
        }

        public class SetHabitLogRequest
        {
            public int HabitId { get; set; }
            public DateTime Date { get; set; }
            public HabitLogStatus Status { get; set; }
        }

        [HttpPost("set-log")]
        public async Task<IActionResult> SetHabitLog([FromBody] SetHabitLogRequest request)
        {
            var userId = GetCurrentUserId();

            var habit = await _context.Habits
                .FirstOrDefaultAsync(h => h.Id == request.HabitId && h.UserId == userId);

            if (habit == null)
            {
                return NotFound();
            }

            var targetDate = request.Date.Date;

            var existingLog = await _context.HabitLogs
                .FirstOrDefaultAsync(hl => hl.HabitId == request.HabitId && hl.LogDate.Date == targetDate);

            if (existingLog == null)
            {
                existingLog = new HabitLog
                {
                    HabitId = request.HabitId,
                    LogDate = targetDate,
                    Status = request.Status
                };

                _context.HabitLogs.Add(existingLog);
            }
            else
            {
                existingLog.Status = request.Status;
            }

            if (targetDate == DateTime.Today)
            {
                if (request.Status == HabitLogStatus.Completed)
                {
                    habit.KanbanStatus = KanbanStatus.Done;
                }
                else if (request.Status == HabitLogStatus.PartiallyCompleted)
                {
                    habit.KanbanStatus = KanbanStatus.InProgress;
                }
                else
                {
                    habit.KanbanStatus = KanbanStatus.Todo;
                }

                if (habit.Status != HabitStatus.Completed)
                {
                    habit.EndDate = null;
                }
            }

            await _context.SaveChangesAsync();

            return Ok();
        }

        public class ClearHabitLogRequest
        {
            public int HabitId { get; set; }
            public DateTime Date { get; set; }
        }

        [HttpPost("clear-log")]
        public async Task<IActionResult> ClearHabitLog([FromBody] ClearHabitLogRequest request)
        {
            var userId = GetCurrentUserId();

            var habit = await _context.Habits
                .FirstOrDefaultAsync(h => h.Id == request.HabitId && h.UserId == userId);

            if (habit == null)
            {
                return NotFound();
            }

            var targetDate = request.Date.Date;

            var existingLog = await _context.HabitLogs
                .FirstOrDefaultAsync(hl => hl.HabitId == request.HabitId && hl.LogDate.Date == targetDate);

            if (existingLog != null)
            {
                _context.HabitLogs.Remove(existingLog);
            }

            if (targetDate == DateTime.Today)
            {
                habit.KanbanStatus = KanbanStatus.Todo;

                if (habit.Status != HabitStatus.Completed)
                {
                    habit.EndDate = null;
                }
            }

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("mark-all-today-completed")]
        public async Task<IActionResult> MarkAllForTodayCompleted()
        {
            var userId = GetCurrentUserId();
            var today = DateTime.Today;

            var habits = await _context.Habits
                .Where(h => h.UserId == userId)
                .OrderBy(h => h.Title)
                .ToListAsync();

            var plannedHabitsForToday = habits
                .Where(h => _scheduleService.IsHabitPlannedForDate(h, today))
                .ToList();

            if (!plannedHabitsForToday.Any())
            {
                return Ok();
            }

            var habitIds = plannedHabitsForToday.Select(h => h.Id).ToList();

            var existingLogs = await _context.HabitLogs
                .Where(hl => habitIds.Contains(hl.HabitId) && hl.LogDate.Date == today)
                .ToListAsync();

            foreach (var habit in plannedHabitsForToday)
            {
                var existingLog = existingLogs.FirstOrDefault(hl => hl.HabitId == habit.Id);

                if (existingLog == null)
                {
                    _context.HabitLogs.Add(new HabitLog
                    {
                        HabitId = habit.Id,
                        LogDate = today,
                        Status = HabitLogStatus.Completed
                    });
                }
                else
                {
                    existingLog.Status = HabitLogStatus.Completed;
                }

                habit.KanbanStatus = KanbanStatus.Done;

                if (habit.Status != HabitStatus.Completed)
                {
                    habit.EndDate = null;
                }
            }

            await _context.SaveChangesAsync();

            return Ok();
        }

        private List<CalendarDayViewModel> BuildCalendarDays(
            DateTime firstDayOfMonth,
            DateTime lastDayOfMonth,
            List<Habit> habits,
            List<HabitLog> habitLogs)
        {
            var result = new List<CalendarDayViewModel>();

            int startOffset = ((int)firstDayOfMonth.DayOfWeek + 6) % 7;
            var calendarStart = firstDayOfMonth.AddDays(-startOffset);

            int endOffset = 6 - (((int)lastDayOfMonth.DayOfWeek + 6) % 7);
            var calendarEnd = lastDayOfMonth.AddDays(endOffset);

            for (var date = calendarStart; date <= calendarEnd; date = date.AddDays(1))
            {
                var logsForDate = habitLogs
                    .Where(hl => hl.LogDate.Date == date.Date)
                    .ToList();

                var plannedHabits = habits
                    .Where(h => _scheduleService.IsHabitPlannedForDate(h, date))
                    .Select(h =>
                    {
                        var log = logsForDate.FirstOrDefault(hl => hl.HabitId == h.Id);

                        return new CalendarHabitItemViewModel
                        {
                            HabitId = h.Id,
                            Title = h.Title,
                            CurrentStatus = log?.Status ?? GetImplicitStatus(h, date)
                        };
                    })
                    .OrderBy(h => h.Title)
                    .ToList();

                var partialHabits = plannedHabits
                    .Where(h => h.CurrentStatus == HabitLogStatus.PartiallyCompleted)
                    .ToList();

                var completedHabits = plannedHabits
                    .Where(h => h.CurrentStatus == HabitLogStatus.Completed)
                    .ToList();

                var skippedHabits = plannedHabits
                    .Where(h => h.CurrentStatus == HabitLogStatus.Skipped)
                    .ToList();

                result.Add(new CalendarDayViewModel
                {
                    Date = date,
                    IsCurrentMonth = date.Month == firstDayOfMonth.Month,
                    IsToday = date.Date == DateTime.Today,
                    PlannedHabits = plannedHabits,
                    PartialHabits = partialHabits,
                    CompletedHabits = completedHabits,
                    SkippedHabits = skippedHabits
                });
            }

            return result;
        }

        private HabitLogStatus? GetImplicitStatus(Habit habit, DateTime date)
        {
            if (habit.Status == HabitStatus.Completed &&
                habit.EndDate.HasValue &&
                habit.EndDate.Value.Date == date.Date)
            {
                return HabitLogStatus.Completed;
            }

            return null;
        }
    }
}
