using System;
using System.Threading.Tasks;
using HabitTracker.Api.Models;
using HabitTracker.Web.Services;
using HabitTracker.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HabitTracker.Web.Pages.Calendar
{
    public class IndexModel : PageModel
    {
        private readonly HabitApiClient _apiClient;

        public IndexModel(HabitApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public CalendarViewModel Calendar { get; set; } = new CalendarViewModel();

        public async Task<IActionResult> OnGetAsync(int? year, int? month)
        {
            var result = await _apiClient.GetCalendarAsync(year, month);
            if (result != null)
            {
                Calendar = result;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostSetHabitLogAsync(int habitId, DateTime date, HabitLogStatus status, int year, int month)
        {
            await _apiClient.SetHabitLogAsync(habitId, date, status);
            return RedirectToPage("/Calendar/Index", new { year, month });
        }

        public async Task<IActionResult> OnPostClearHabitLogAsync(int habitId, DateTime date, int year, int month)
        {
            await _apiClient.ClearHabitLogAsync(habitId, date);
            return RedirectToPage("/Calendar/Index", new { year, month });
        }

        public async Task<IActionResult> OnPostMarkAllForTodayCompletedAsync(int year, int month)
        {
            await _apiClient.MarkAllTodayCompletedAsync();
            return RedirectToPage("/Calendar/Index", new { year, month });
        }
    }
}
