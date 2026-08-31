using System.Threading.Tasks;
using HabitTracker.Api.Models;
using HabitTracker.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HabitTracker.Web.Pages.Habits
{
    public class DeleteModel : PageModel
    {
        private readonly HabitApiClient _apiClient;

        public DeleteModel(HabitApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [BindProperty]
        public Habit Habit { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var habit = await _apiClient.GetHabitByIdAsync(id.Value);
            if (habit == null)
            {
                return NotFound();
            }

            Habit = habit;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var success = await _apiClient.DeleteHabitAsync(id);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Unable to delete habit via API.");
                return Page();
            }

            TempData["SuccessMessage"] = "Habit deleted successfully.";
            return RedirectToPage("/Habits/Index");
        }
    }
}
