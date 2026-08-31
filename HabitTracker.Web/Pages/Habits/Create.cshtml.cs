using System.Threading.Tasks;
using HabitTracker.Api.Models;
using HabitTracker.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HabitTracker.Web.Pages.Habits
{
    public class CreateModel : PageModel
    {
        private readonly HabitApiClient _apiClient;

        public CreateModel(HabitApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [BindProperty]
        public Habit Habit { get; set; } = new Habit();

        public IActionResult OnGet()
        {
            Habit.StartDate = System.DateTime.Today;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var success = await _apiClient.CreateHabitAsync(Habit);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Unable to create habit via API.");
                return Page();
            }

            TempData["SuccessMessage"] = "Habit created successfully.";
            return RedirectToPage("/Habits/Index");
        }
    }
}
