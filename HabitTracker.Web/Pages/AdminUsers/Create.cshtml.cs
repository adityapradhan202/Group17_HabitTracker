using System.Threading.Tasks;
using HabitTracker.Web.Services;
using HabitTracker.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HabitTracker.Web.Pages.AdminUsers
{
    public class CreateModel : PageModel
    {
        private readonly AdminApiClient _adminApiClient;

        public CreateModel(AdminApiClient adminApiClient)
        {
            _adminApiClient = adminApiClient;
        }

        [BindProperty]
        public AdminUserCreateViewModel UserInput { get; set; } = new AdminUserCreateViewModel();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var success = await _adminApiClient.CreateUserAsync(UserInput);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Unable to create user via API.");
                return Page();
            }

            TempData["SuccessMessage"] = "User created successfully.";
            return RedirectToPage("/AdminUsers/Index");
        }
    }
}
