using System.Threading.Tasks;
using HabitTracker.Web.Services;
using HabitTracker.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HabitTracker.Web.Pages.AdminUsers
{
    public class EditModel : PageModel
    {
        private readonly AdminApiClient _adminApiClient;

        public EditModel(AdminApiClient adminApiClient)
        {
            _adminApiClient = adminApiClient;
        }

        [BindProperty]
        public AdminUserEditViewModel UserInput { get; set; } = new AdminUserEditViewModel();

        public async Task<IActionResult> OnGetAsync(string? id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var userModel = await _adminApiClient.GetUserByIdAsync(id);
            if (userModel == null)
            {
                return NotFound();
            }

            UserInput = userModel;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            if (id != UserInput.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var success = await _adminApiClient.UpdateUserAsync(id, UserInput);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Unable to update user via API.");
                return Page();
            }

            TempData["SuccessMessage"] = "User updated successfully.";
            return RedirectToPage("/AdminUsers/Index");
        }
    }
}
