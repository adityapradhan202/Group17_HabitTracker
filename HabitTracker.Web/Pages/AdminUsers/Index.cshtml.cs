using System.Collections.Generic;
using System.Threading.Tasks;
using HabitTracker.Web.Services;
using HabitTracker.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HabitTracker.Web.Pages.AdminUsers
{
    public class IndexModel : PageModel
    {
        private readonly AdminApiClient _adminApiClient;

        public IndexModel(AdminApiClient adminApiClient)
        {
            _adminApiClient = adminApiClient;
        }

        public List<AdminUserListItemViewModel> Users { get; set; } = new List<AdminUserListItemViewModel>();

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            Users = await _adminApiClient.GetUsersAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostActivateAsync(string id)
        {
            var success = await _adminApiClient.ActivateUserAsync(id);
            if (success)
            {
                SuccessMessage = "User activated successfully.";
            }
            else
            {
                ErrorMessage = "Failed to activate user.";
            }
            return RedirectToPage("/AdminUsers/Index");
        }
    }
}
