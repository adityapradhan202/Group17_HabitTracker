using System.Security.Claims;
using HabitTracker.Api.Data;
using HabitTracker.Api.Models;
using HabitTracker.Api.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Api.Controllers
{
    [ApiController]
    [Route("api/admin/users")]
    [Authorize(Roles = "Admin")]
    public class AdminUsersController : ControllerBase
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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AdminUserListItemViewModel>>> GetUsers()
        {
            var users = await _context.Users
                .OrderBy(u => u.Email)
                .ToListAsync();

            var habitCounts = await _context.Habits
                .GroupBy(h => h.UserId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.UserId, x => x.Count);

            var result = new List<AdminUserListItemViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                result.Add(new AdminUserListItemViewModel
                {
                    UserId = user.Id,
                    Email = user.Email ?? user.UserName ?? user.Id,
                    HabitCount = habitCounts.TryGetValue(user.Id, out var count) ? count : 0,
                    IsActive = user.IsActive,
                    RoleName = roles.FirstOrDefault() ?? "User"
                });
            }

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AdminUserEditViewModel>> GetUser(string id)
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

            return Ok(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] AdminUserCreateViewModel model)
        {
            if (!await _roleManager.RoleExistsAsync(model.RoleName))
            {
                ModelState.AddModelError(nameof(model.RoleName), "Selected role was not found.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
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
                return BadRequest(ModelState);
            }

            var roleResult = await _userManager.AddToRoleAsync(user, model.RoleName);

            if (!roleResult.Succeeded)
            {
                foreach (var error in roleResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return BadRequest(ModelState);
            }

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, model);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] AdminUserEditViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest("ID mismatch");
            }

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
                return BadRequest(ModelState);
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
                return BadRequest(ModelState);
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
                    return BadRequest(ModelState);
                }
            }

            var addRoleResult = await _userManager.AddToRoleAsync(user, model.RoleName);

            if (!addRoleResult.Succeeded)
            {
                foreach (var error in addRoleResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return BadRequest(ModelState);
            }

            return NoContent();
        }

        [HttpPost("{id}/activate")]
        public async Task<IActionResult> ActivateUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            user.IsActive = true;
            await _userManager.UpdateAsync(user);

            return Ok();
        }

        [HttpPost("{id}/deactivate")]
        public async Task<IActionResult> DeactivateUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            if (user.Id == GetCurrentUserId())
            {
                return BadRequest("You cannot deactivate your own account.");
            }

            user.IsActive = false;
            await _userManager.UpdateAsync(user);

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            if (user.Id == GetCurrentUserId())
            {
                return BadRequest("You cannot delete your own account.");
            }

            var deleteResult = await _userManager.DeleteAsync(user);
            if (!deleteResult.Succeeded)
            {
                var errors = string.Join("; ", deleteResult.Errors.Select(e => e.Description));
                return BadRequest(errors);
            }

            return NoContent();
        }
    }
}
