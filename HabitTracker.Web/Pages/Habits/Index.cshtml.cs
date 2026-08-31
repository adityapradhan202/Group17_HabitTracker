using System.Collections.Generic;
using System.Threading.Tasks;
using HabitTracker.Api.Models;
using HabitTracker.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HabitTracker.Web.Pages.Habits
{
    public class IndexModel : PageModel
    {
        private readonly HabitApiClient _apiClient;

        public IndexModel(HabitApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public List<Habit> Habits { get; set; } = new List<Habit>();

        [TempData]
        public string? SuccessMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            Habits = await _apiClient.GetHabitsAsync();
            return Page();
        }
    }
}
