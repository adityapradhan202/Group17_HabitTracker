using System.Security.Claims;
using HabitTracker.Api.Data;
using HabitTracker.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Api.Controllers
{
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("/")]
        public IActionResult Root()
        {
            return Ok(new
            {
                Status = "Online",
                Service = "HabitTracker.Api",
                Message = "Backend Web API is running."
            });
        }

        [HttpGet("api/[controller]")]
        public async Task<ActionResult<IEnumerable<Habit>>> GetDashboardHabits()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return Ok(new List<Habit>());
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                return Ok(new List<Habit>());
            }

            var habits = await _context.Habits
                .Include(h => h.User)
                .Where(h => h.UserId == currentUserId)
                .OrderByDescending(h => h.StartDate)
                .ToListAsync();

            return Ok(habits);
        }
    }
}
