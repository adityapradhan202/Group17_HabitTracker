using System.Threading.Tasks;
using HabitTracker.Api.Models;
using HabitTracker.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HabitTracker.Web.Pages.Habits
{
    public class DetailsModel : PageModel
    {
        private readonly HabitApiClient _apiClient;

        public DetailsModel(HabitApiClient apiClient)
        {
            _apiClient = apiClient;
        }

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
    }
}
