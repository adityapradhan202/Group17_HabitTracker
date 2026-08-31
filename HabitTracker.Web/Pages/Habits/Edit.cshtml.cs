using System.Threading.Tasks;
using HabitTracker.Api.Models;
using HabitTracker.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HabitTracker.Web.Pages.Habits
{
    public class EditModel : PageModel
    {
        private readonly HabitApiClient _apiClient;

        public EditModel(HabitApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [BindProperty]
        public Habit Habit { get; set; } = new Habit();

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
            if (id != Habit.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var success = await _apiClient.UpdateHabitAsync(id, Habit);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Unable to update habit via API.");
                return Page();
            }

            TempData["SuccessMessage"] = "Habit updated successfully.";
            return RedirectToPage("/Habits/Index");
        }
    }
}
