using System.Security.Claims;
using HabitTrackerAspNetMVCWebApp.Data;
using HabitTrackerAspNetMVCWebApp.Models;
using HabitTrackerAspNetMVCWebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HabitTrackerAspNetMVCWebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminUsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminUsersController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }

        private async Task PopulateRolesAsync(string? selectedRole = null)
        {
            var roles = await _roleManager.Roles
                .OrderBy(r => r.Name)
                .Select(r => r.Name!)
                .ToListAsync();

            ViewBag.RoleName = new SelectList(roles, selectedRole);
        }

        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .OrderBy(u => u.Email)
                .ToListAsync();

            var habitCounts = await _context.Habits
                .GroupBy(h => h.UserId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.UserId, x => x.Count);

            var viewModel = new List<AdminUserListItemViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                viewModel.Add(new AdminUserListItemViewModel
                {
                    UserId = user.Id,
                    Email = user.Email ?? user.UserName ?? user.Id,
                    HabitCount = habitCounts.TryGetValue(user.Id, out var count) ? count : 0,
                    IsActive = user.IsActive,
                    RoleName = roles.FirstOrDefault() ?? "User"
                });
            }

            return View(viewModel);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateRolesAsync("User");
            return View(new AdminUserCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminUserCreateViewModel model)
        {
            if (!await _roleManager.RoleExistsAsync(model.RoleName))
            {
                ModelState.AddModelError(nameof(model.RoleName), "Selected role was not found.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateRolesAsync(model.RoleName);
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                IsActive = model.IsActive,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(user, model.Password);

            if (!createResult.Succeeded)
            {
                foreach (var error in createResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                await PopulateRolesAsync(model.RoleName);
                return View(model);
            }

            var roleResult = await _userManager.AddToRoleAsync(user, model.RoleName);

            if (!roleResult.Succeeded)
            {
                foreach (var error in roleResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                await PopulateRolesAsync(model.RoleName);
                return View(model);
            }

            TempData["SuccessMessage"] = "User created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(string? id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);

            var model = new AdminUserEditViewModel
            {
                Id = user.Id,
                Email = user.Email ?? user.UserName ?? string.Empty,
                RoleName = roles.FirstOrDefault() ?? "User",
                IsActive = user.IsActive
            };

            await PopulateRolesAsync(model.RoleName);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AdminUserEditViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);

            if (user == null)
            {
                return NotFound();
            }

            if (!await _roleManager.RoleExistsAsync(model.RoleName))
            {
                ModelState.AddModelError(nameof(model.RoleName), "Selected role was not found.");
            }

            var currentUserId = GetCurrentUserId();
            if (user.Id == currentUserId && !model.IsActive)
            {
                ModelState.AddModelError(nameof(model.IsActive), "You cannot deactivate your own account.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateRolesAsync(model.RoleName);
                return View(model);
            }

            user.Email = model.Email;
            user.UserName = model.Email;
            user.IsActive = model.IsActive;

            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                await PopulateRolesAsync(model.RoleName);
                return View(model);
            }

            var currentRoles = await _userManager.GetRolesAsync(user);

            if (currentRoles.Any())
            {
                var removeRolesResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);

                if (!removeRolesResult.Succeeded)
                {
                    foreach (var error in removeRolesResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }

                    await PopulateRolesAsync(model.RoleName);
                    return View(model);
                }
            }

            var addRoleResult = await _userManager.AddToRoleAsync(user, model.RoleName);

            if (!addRoleResult.Succeeded)
            {
                foreach (var error in addRoleResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                await PopulateRolesAsync(model.RoleName);
                return View(model);
            }

            TempData["SuccessMessage"] = "User updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Deactivate(string? id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);

            var habitCount = await _context.Habits.CountAsync(h => h.UserId == user.Id);

            var model = new AdminUserListItemViewModel
            {
                UserId = user.Id,
                Email = user.Email ?? user.UserName ?? user.Id,
                HabitCount = habitCount,
                IsActive = user.IsActive,
                RoleName = roles.FirstOrDefault() ?? "User"
            };

            return View(model);
        }

        [HttpPost, ActionName("Deactivate")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeactivateConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            if (user.Id == GetCurrentUserId())
            {
                TempData["ErrorMessage"] = "You cannot deactivate your own account.";
                return RedirectToAction(nameof(Index));
            }

            user.IsActive = false;
            await _userManager.UpdateAsync(user);

            TempData["SuccessMessage"] = "User deactivated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            user.IsActive = true;
            await _userManager.UpdateAsync(user);

            TempData["SuccessMessage"] = "User activated successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}