using System.Security.Claims;
using HabitTrackerAspNetMVCWebApp.Data;
using HabitTrackerAspNetMVCWebApp.Models;
using HabitTrackerAspNetMVCWebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HabitTrackerAspNetMVCWebApp.Controllers
{
    [Authorize]
    public class HabitsController : Controller
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

        private bool IsAdmin()
        {
            return User.IsInRole("Admin");
        }

        private async Task<Habit?> GetAccessibleHabitAsync(int id)
        {
            var currentUserId = GetCurrentUserId();

            if (IsAdmin())
            {
                return await _context.Habits
                    .Include(h => h.User)
                    .FirstOrDefaultAsync(h => h.Id == id);
            }

            return await _context.Habits
                .Include(h => h.User)
                .FirstOrDefaultAsync(h => h.Id == id && h.UserId == currentUserId);
        }

        private async Task PopulateUsersDropDownAsync(string? selectedUserId = null)
        {
            var users = await _context.Users
                .Where(u => u.IsActive)
                .OrderBy(u => u.Email)
                .Select(u => new
                {
                    u.Id,
                    Display = u.Email ?? u.UserName ?? u.Id
                })
                .ToListAsync();

            ViewBag.Users = new SelectList(users, "Id", "Display", selectedUserId);
        }

        public async Task<IActionResult> Index(string? userId = null, bool all = false)
        {
            var currentUserId = GetCurrentUserId();
            var isAdmin = IsAdmin();

            IQueryable<Habit> query = _context.Habits
                .Include(h => h.User);

            if (isAdmin)
            {
                if (all)
                {
                    ViewBag.PageTitle = "All Habits";
                }
                else if (!string.IsNullOrEmpty(userId))
                {
                    query = query.Where(h => h.UserId == userId);

                    var selectedUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                    ViewBag.PageTitle = selectedUser?.Email != null
                        ? $"Habits for {selectedUser.Email}"
                        : "Habits for Selected User";
                }
                else
                {
                    query = query.Where(h => h.UserId == currentUserId);
                    ViewBag.PageTitle = "My Habits";
                }
            }
            else
            {
                query = query.Where(h => h.UserId == currentUserId);
                ViewBag.PageTitle = "My Habits";
            }

            var habits = await query
                .OrderBy(h => h.StartDate)
                .ThenBy(h => h.Title)
                .ToListAsync();

            ViewBag.IsAdmin = isAdmin;
            ViewBag.SelectedUserId = userId;
            ViewBag.ShowingAll = all;

            return View(habits);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminUsers()
        {
            var users = await _context.Users
                .OrderBy(u => u.Email)
                .ToListAsync();

            var habitCounts = await _context.Habits
                .GroupBy(h => h.UserId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.UserId, x => x.Count);

            var viewModel = users
                .Select(u => new AdminUserListItemViewModel
                {
                    UserId = u.Id,
                    Email = u.Email ?? u.UserName ?? u.Id,
                    HabitCount = habitCounts.TryGetValue(u.Id, out var count) ? count : 0,
                    IsActive = u.IsActive
                })
                .ToList();

            return View(viewModel);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var habit = await GetAccessibleHabitAsync(id.Value);

            if (habit == null)
            {
                return NotFound();
            }

            ViewBag.IsAdmin = IsAdmin();
            return View(habit);
        }

        public async Task<IActionResult> Create(string? userId = null)
        {
            if (IsAdmin())
            {
                await PopulateUsersDropDownAsync(userId);
                ViewBag.SelectedUserId = userId;
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Frequency,Status,KanbanStatus,StartDate,EndDate")] Habit habit, string? ownerUserId)
        {
            var isAdmin = IsAdmin();
            var currentUserId = GetCurrentUserId();

            if (isAdmin)
            {
                if (string.IsNullOrEmpty(ownerUserId))
                {
                    ModelState.AddModelError("ownerUserId", "Please select a user.");
                }
                else
                {
                    var userExists = await _context.Users.AnyAsync(u => u.Id == ownerUserId && u.IsActive);
                    if (!userExists)
                    {
                        ModelState.AddModelError("ownerUserId", "Selected user was not found.");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                if (isAdmin)
                {
                    await PopulateUsersDropDownAsync(ownerUserId);
                    ViewBag.SelectedUserId = ownerUserId;
                }

                return View(habit);
            }

            habit.UserId = isAdmin ? ownerUserId! : currentUserId;

            _context.Habits.Add(habit);
            await _context.SaveChangesAsync();

            if (isAdmin)
            {
                return RedirectToAction(nameof(Index), new { userId = habit.UserId });
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var habit = await GetAccessibleHabitAsync(id.Value);

            if (habit == null)
            {
                return NotFound();
            }

            if (IsAdmin())
            {
                await PopulateUsersDropDownAsync(habit.UserId);
                ViewBag.SelectedUserId = habit.UserId;
            }

            return View(habit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Frequency,Status,KanbanStatus,StartDate,EndDate")] Habit habit, string? ownerUserId)
        {
            if (id != habit.Id)
            {
                return NotFound();
            }

            var existingHabit = await GetAccessibleHabitAsync(id);

            if (existingHabit == null)
            {
                return NotFound();
            }

            var isAdmin = IsAdmin();

            if (isAdmin)
            {
                if (string.IsNullOrEmpty(ownerUserId))
                {
                    ModelState.AddModelError("ownerUserId", "Please select a user.");
                }
                else
                {
                    var userExists = await _context.Users.AnyAsync(u => u.Id == ownerUserId && u.IsActive);
                    if (!userExists)
                    {
                        ModelState.AddModelError("ownerUserId", "Selected user was not found.");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                if (isAdmin)
                {
                    await PopulateUsersDropDownAsync(ownerUserId ?? existingHabit.UserId);
                    ViewBag.SelectedUserId = ownerUserId ?? existingHabit.UserId;
                }

                return View(habit);
            }

            existingHabit.Title = habit.Title;
            existingHabit.Description = habit.Description;
            existingHabit.Frequency = habit.Frequency;
            existingHabit.Status = habit.Status;
            existingHabit.KanbanStatus = habit.KanbanStatus;
            existingHabit.StartDate = habit.StartDate;
            existingHabit.EndDate = habit.EndDate;

            if (isAdmin && !string.IsNullOrEmpty(ownerUserId))
            {
                existingHabit.UserId = ownerUserId;
            }

            try
            {
                _context.Update(existingHabit);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Habits.AnyAsync(h => h.Id == id))
                {
                    return NotFound();
                }

                throw;
            }

            if (isAdmin)
            {
                return RedirectToAction(nameof(Index), new { userId = existingHabit.UserId });
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var habit = await GetAccessibleHabitAsync(id.Value);

            if (habit == null)
            {
                return NotFound();
            }

            ViewBag.IsAdmin = IsAdmin();
            return View(habit);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var habit = await GetAccessibleHabitAsync(id);

            if (habit != null)
            {
                var ownerUserId = habit.UserId;

                _context.Habits.Remove(habit);
                await _context.SaveChangesAsync();

                if (IsAdmin())
                {
                    return RedirectToAction(nameof(Index), new { userId = ownerUserId });
                }
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CleanupOrphans()
        {
            var orphanHabits = _context.Habits
                .Where(h => string.IsNullOrEmpty(h.UserId) ||
                            !_context.Users.Any(u => u.Id == h.UserId));

            var count = await orphanHabits.CountAsync();

            _context.Habits.RemoveRange(orphanHabits);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Removed {count} orphan habits.";
            return RedirectToAction(nameof(Index), new { all = true });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SeedDemoData()
        {
            await DemoDataSeeder.SeedAsync(_serviceProvider);
            TempData["SuccessMessage"] = "Demo data seeded successfully.";
            return RedirectToAction(nameof(Index), new { all = true });
        }
    }
}