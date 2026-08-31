using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using HabitTracker.Api.Models;
using HabitTracker.Web.ViewModels;

namespace HabitTracker.Web.Services
{
    public class HabitApiClient
    {
        private readonly HttpClient _client;

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public HabitApiClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<List<Habit>> GetDashboardHabitsAsync()
        {
            var response = await _client.GetAsync("/api/home");
            if (!response.IsSuccessStatusCode)
                return new List<Habit>();

            return await response.Content.ReadFromJsonAsync<List<Habit>>(JsonOptions) ?? new List<Habit>();
        }

        public async Task<List<Habit>> GetHabitsAsync()
        {
            var response = await _client.GetAsync("/api/habits");
            if (!response.IsSuccessStatusCode)
                return new List<Habit>();

            return await response.Content.ReadFromJsonAsync<List<Habit>>(JsonOptions) ?? new List<Habit>();
        }

        public async Task<Habit?> GetHabitByIdAsync(int id)
        {
            var response = await _client.GetAsync($"/api/habits/{id}");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<Habit>(JsonOptions);
        }

        public async Task<bool> CreateHabitAsync(Habit habit)
        {
            var response = await _client.PostAsJsonAsync("/api/habits", habit, JsonOptions);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateHabitAsync(int id, Habit habit)
        {
            var response = await _client.PutAsJsonAsync($"/api/habits/{id}", habit, JsonOptions);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteHabitAsync(int id)
        {
            var response = await _client.DeleteAsync($"/api/habits/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<CalendarViewModel?> GetCalendarAsync(int? year, int? month)
        {
            var url = "/api/calendar";
            if (year.HasValue && month.HasValue)
            {
                url += $"?year={year.Value}&month={month.Value}";
            }

            var response = await _client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<CalendarViewModel>(JsonOptions);
        }

        public async Task<bool> SetHabitLogAsync(int habitId, DateTime date, HabitLogStatus status)
        {
            var response = await _client.PostAsJsonAsync("/api/calendar/set-log", new
            {
                habitId,
                date,
                status
            }, JsonOptions);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ClearHabitLogAsync(int habitId, DateTime date)
        {
            var response = await _client.PostAsJsonAsync("/api/calendar/clear-log", new
            {
                habitId,
                date
            }, JsonOptions);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> MarkAllTodayCompletedAsync()
        {
            var response = await _client.PostAsync("/api/calendar/mark-all-today-completed", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<KanbanBoardViewModel?> GetKanbanBoardAsync()
        {
            var response = await _client.GetAsync("/api/kanban");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<KanbanBoardViewModel>(JsonOptions);
        }

        public async Task<bool> StartTodayAsync(int id)
        {
            var response = await _client.PostAsync($"/api/kanban/start-today/{id}", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> MarkTodayCompleteAsync(int id)
        {
            var response = await _client.PostAsync($"/api/kanban/mark-today-complete/{id}", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> MoveToTodoAsync(int id)
        {
            var response = await _client.PostAsync($"/api/kanban/move-to-todo/{id}", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> CompleteHabitAsync(int id)
        {
            var response = await _client.PostAsync($"/api/kanban/complete-habit/{id}", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ReopenHabitAsync(int id)
        {
            var response = await _client.PostAsync($"/api/kanban/reopen-habit/{id}", null);
            return response.IsSuccessStatusCode;
        }
    }
}
