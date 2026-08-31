using System.Threading.Tasks;
using HabitTracker.Web.Services;
using HabitTracker.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HabitTracker.Web.Pages.AdminUsers
{
    public class DeactivateModel : PageModel
    {
        private readonly AdminApiClient _adminApiClient;

        public DeactivateModel(AdminApiClient adminApiClient)
        {
            _adminApiClient = adminApiClient;
        }

        [BindProperty]
        public AdminUserListItemViewModel UserInfo { get; set; } = new AdminUserListItemViewModel();

        public async Task<IActionResult> OnGetAsync(string? id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var editModel = await _adminApiClient.GetUserByIdAsync(id);
            if (editModel == null)
            {
                return NotFound();
            }

            UserInfo = new AdminUserListItemViewModel
            {
                UserId = editModel.Id,
                Email = editModel.Email,
                RoleName = editModel.RoleName,
                IsActive = editModel.IsActive
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            var success = await _adminApiClient.DeactivateUserAsync(id);
            if (!success)
            {
                TempData["ErrorMessage"] = "Unable to deactivate user.";
                return RedirectToPage("/AdminUsers/Index");
            }

            TempData["SuccessMessage"] = "User deactivated successfully.";
            return RedirectToPage("/AdminUsers/Index");
        }
    }
}
