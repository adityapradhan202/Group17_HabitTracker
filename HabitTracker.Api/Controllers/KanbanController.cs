using System.Security.Claims;
using HabitTracker.Api.Data;
using HabitTracker.Api.Models;
using HabitTracker.Api.Services;
using HabitTracker.Api.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "User")]
    public class KanbanController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly HabitScheduleService _scheduleService;

        public KanbanController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            HabitScheduleService scheduleService)
        {
            _context = context;
            _userManager = userManager;
            _scheduleService = scheduleService;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }

        [HttpGet]
        public async Task<ActionResult<KanbanBoardViewModel>> GetKanbanBoard()
        {
            var userId = GetCurrentUserId();

            var habits = await _context.Habits
                .Where(h => h.UserId == userId)
                .Include(h => h.HabitLogs)
                .OrderBy(h => h.Title)
                .ToListAsync();

            var viewModel = new KanbanBoardViewModel
            {
                TodoHabits = habits
                    .Where(h => h.KanbanStatus == KanbanStatus.Todo)
                    .OrderBy(h => h.Title)
                    .ToList(),

                InProgressHabits = habits
                    .Where(h => h.KanbanStatus == KanbanStatus.InProgress)
                    .OrderBy(h => h.Title)
                    .ToList(),

                DoneHabits = habits
                    .Where(h => h.KanbanStatus == KanbanStatus.Done)
                    .OrderBy(h => h.Title)
                    .ToList()
            };

            return Ok(viewModel);
        }

        [HttpPost("start-today/{id}")]
        public async Task<IActionResult> StartToday(int id)
        {
            var userId = GetCurrentUserId();

            var habit = await _context.Habits
                .FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId);

            if (habit == null)
            {
                return NotFound();
            }

            var today = DateTime.Today;

            if (!_scheduleService.IsHabitPlannedForDate(habit, today))
            {
                return Ok();
            }

            var existingLog = await _context.HabitLogs
                .FirstOrDefaultAsync(hl => hl.HabitId == habit.Id && hl.LogDate.Date == today);

            if (existingLog == null)
            {
                _context.HabitLogs.Add(new HabitLog
                {
                    HabitId = habit.Id,
                    LogDate = today,
                    Status = HabitLogStatus.PartiallyCompleted
                });
            }
            else
            {
                existingLog.Status = HabitLogStatus.PartiallyCompleted;
            }

            if (habit.Status != HabitStatus.Completed)
            {
                habit.EndDate = null;
            }

            habit.KanbanStatus = KanbanStatus.InProgress;

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("mark-today-complete/{id}")]
        public async Task<IActionResult> MarkTodayComplete(int id)
        {
            var userId = GetCurrentUserId();

            var habit = await _context.Habits
                .FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId);

            if (habit == null)
            {
                return NotFound();
            }

            var today = DateTime.Today;

            if (!_scheduleService.IsHabitPlannedForDate(habit, today))
            {
                return Ok();
            }

            var existingLog = await _context.HabitLogs
                .FirstOrDefaultAsync(hl => hl.HabitId == habit.Id && hl.LogDate.Date == today);

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

            if (habit.Status != HabitStatus.Completed)
            {
                habit.EndDate = null;
            }

            habit.KanbanStatus = KanbanStatus.Done;

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("move-to-todo/{id}")]
        public async Task<IActionResult> MoveToTodo(int id)
        {
            var userId = GetCurrentUserId();

            var habit = await _context.Habits
                .FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId);

            if (habit == null)
            {
                return NotFound();
            }

            var today = DateTime.Today;

            var existingLog = await _context.HabitLogs
                .FirstOrDefaultAsync(hl => hl.HabitId == habit.Id && hl.LogDate.Date == today);

            if (existingLog != null)
            {
                _context.HabitLogs.Remove(existingLog);
            }

            if (habit.Status != HabitStatus.Completed)
            {
                habit.EndDate = null;
            }

            habit.KanbanStatus = KanbanStatus.Todo;

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("complete-habit/{id}")]
        public async Task<IActionResult> CompleteHabit(int id)
        {
            var userId = GetCurrentUserId();

            var habit = await _context.Habits
                .FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId);

            if (habit == null)
            {
                return NotFound();
            }

            var today = DateTime.Today;

            habit.Status = HabitStatus.Completed;
            habit.EndDate = today;
            habit.KanbanStatus = KanbanStatus.Done;

            if (_scheduleService.IsHabitPlannedForDate(habit, today))
            {
                var existingLog = await _context.HabitLogs
                    .FirstOrDefaultAsync(hl => hl.HabitId == habit.Id && hl.LogDate.Date == today);

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
            }

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("reopen-habit/{id}")]
        public async Task<IActionResult> ReopenHabit(int id)
        {
            var userId = GetCurrentUserId();

            var habit = await _context.Habits
                .FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId);

            if (habit == null)
            {
                return NotFound();
            }

            habit.Status = HabitStatus.Active;
            habit.EndDate = null;
            habit.KanbanStatus = KanbanStatus.InProgress;

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
