using System.Security.Claims;
using HabitTracker.Api.Data;
using HabitTracker.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "User")]
    public class HabitsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IServiceProvider _serviceProvider;

        public HabitsController(ApplicationDbContext context, IServiceProvider serviceProvider)
        {
            _context = context;
            _serviceProvider = serviceProvider;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Habit>>> GetHabits()
        {
            var currentUserId = GetCurrentUserId();

            var habits = await _context.Habits
                .Include(h => h.User)
                .Where(h => h.UserId == currentUserId)
                .OrderBy(h => h.StartDate)
                .ThenBy(h => h.Title)
                .ToListAsync();

            return Ok(habits);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Habit>> GetHabit(int id)
        {
            var currentUserId = GetCurrentUserId();

            var habit = await _context.Habits
                .Include(h => h.User)
                .FirstOrDefaultAsync(h => h.Id == id && h.UserId == currentUserId);

            if (habit == null)
            {
                return NotFound();
            }

            return Ok(habit);
        }

        [HttpPost]
        public async Task<ActionResult<Habit>> CreateHabit([FromBody] Habit habit)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = GetCurrentUserId();
            habit.UserId = currentUserId;
            habit.User = null; // Prevent navigation object tracking issues

            _context.Habits.Add(habit);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetHabit), new { id = habit.Id }, habit);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateHabit(int id, [FromBody] Habit habit)
        {
            if (id != habit.Id)
            {
                return BadRequest("ID mismatch");
            }

            var currentUserId = GetCurrentUserId();

            var existingHabit = await _context.Habits
                .FirstOrDefaultAsync(h => h.Id == id && h.UserId == currentUserId);

            if (existingHabit == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            existingHabit.Title = habit.Title;
            existingHabit.Description = habit.Description;
            existingHabit.Frequency = habit.Frequency;
            existingHabit.Status = habit.Status;
            existingHabit.KanbanStatus = habit.KanbanStatus;
            existingHabit.StartDate = habit.StartDate;
            existingHabit.EndDate = habit.EndDate;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Habits.AnyAsync(h => h.Id == id && h.UserId == currentUserId))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHabit(int id)
        {
            var currentUserId = GetCurrentUserId();

            var habit = await _context.Habits
                .FirstOrDefaultAsync(h => h.Id == id && h.UserId == currentUserId);

            if (habit == null)
            {
                return NotFound();
            }

            _context.Habits.Remove(habit);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
