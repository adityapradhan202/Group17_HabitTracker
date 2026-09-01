using System.Threading.Tasks;
using HabitTracker.Web.Services;
using HabitTracker.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HabitTracker.Web.Pages.Kanban
{
    public class IndexModel : PageModel
    {
        private readonly HabitApiClient _apiClient;

        public IndexModel(HabitApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public KanbanBoardViewModel Kanban { get; set; } = new KanbanBoardViewModel();

        public async Task<IActionResult> OnGetAsync()
        {
            var board = await _apiClient.GetKanbanBoardAsync();
            if (board != null)
            {
                Kanban = board;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostStartTodayAsync(int id)
        {
            await _apiClient.StartTodayAsync(id);
            return RedirectToPage("/Kanban/Index");
        }

        public async Task<IActionResult> OnPostMarkTodayCompleteAsync(int id)
        {
            await _apiClient.MarkTodayCompleteAsync(id);
            return RedirectToPage("/Kanban/Index");
        }

        public async Task<IActionResult> OnPostMoveToTodoAsync(int id)
        {
            await _apiClient.MoveToTodoAsync(id);
            return RedirectToPage("/Kanban/Index");
        }

        public async Task<IActionResult> OnPostCompleteHabitAsync(int id)
        {
            await _apiClient.CompleteHabitAsync(id);
            return RedirectToPage("/Kanban/Index");
        }

        public async Task<IActionResult> OnPostReopenHabitAsync(int id)
        {
            await _apiClient.ReopenHabitAsync(id);
            return RedirectToPage("/Kanban/Index");
        }
    }
}
