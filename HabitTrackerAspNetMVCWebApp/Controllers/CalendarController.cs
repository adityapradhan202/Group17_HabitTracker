using HabitTrackerAspNetMVCWebApp.Data;
using HabitTrackerAspNetMVCWebApp.Models;
using HabitTrackerAspNetMVCWebApp.Services;
using HabitTrackerAspNetMVCWebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HabitTrackerAspNetMVCWebApp.Controllers
{
    [Authorize]
    public class CalendarController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly HabitScheduleService _scheduleService;

        public CalendarController(ApplicationDbContext context)
        {
            _context = context;
            _scheduleService = new HabitScheduleService();
        }

        public async Task<IActionResult> Index(int? year, int? month)
        {
            var today = DateTime.Today;
            int selectedYear = year ?? today.Year;
            int selectedMonth = month ?? today.Month;

            var firstDayOfMonth = new DateTime(selectedYear, selectedMonth, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

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

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetHabitLog(int habitId, DateTime date, HabitLogStatus status, int year, int month)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var habit = await _context.Habits
                .FirstOrDefaultAsync(h => h.Id == habitId && h.UserId == userId);

            if (habit == null)
            {
                return NotFound();
            }

            var targetDate = date.Date;

            var existingLog = await _context.HabitLogs
                .FirstOrDefaultAsync(hl => hl.HabitId == habitId && hl.LogDate.Date == targetDate);

            if (existingLog == null)
            {
                existingLog = new HabitLog
                {
                    HabitId = habitId,
                    LogDate = targetDate,
                    Status = status
                };

                _context.HabitLogs.Add(existingLog);
            }
            else
            {
                existingLog.Status = status;
            }

            if (targetDate == DateTime.Today)
            {
                if (status == HabitLogStatus.Completed)
                {
                    habit.KanbanStatus = KanbanStatus.Done;
                }
                else if (status == HabitLogStatus.PartiallyCompleted)
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

            return RedirectToAction(nameof(Index), new { year, month });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearHabitLog(int habitId, DateTime date, int year, int month)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var habit = await _context.Habits
                .FirstOrDefaultAsync(h => h.Id == habitId && h.UserId == userId);

            if (habit == null)
            {
                return NotFound();
            }

            var targetDate = date.Date;

            var existingLog = await _context.HabitLogs
                .FirstOrDefaultAsync(hl => hl.HabitId == habitId && hl.LogDate.Date == targetDate);

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

            return RedirectToAction(nameof(Index), new { year, month });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllForTodayCompleted(int year, int month)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
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
                return RedirectToAction(nameof(Index), new { year, month });
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

            return RedirectToAction(nameof(Index), new { year, month });
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