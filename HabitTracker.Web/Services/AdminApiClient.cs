using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using HabitTracker.Web.ViewModels;

namespace HabitTracker.Web.Services
{
    public class AdminApiClient
    {
        private readonly HttpClient _client;

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public AdminApiClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<List<AdminUserListItemViewModel>> GetUsersAsync()
        {
            var response = await _client.GetAsync("/api/admin/users");
            if (!response.IsSuccessStatusCode)
                return new List<AdminUserListItemViewModel>();

            return await response.Content.ReadFromJsonAsync<List<AdminUserListItemViewModel>>(JsonOptions) ?? new List<AdminUserListItemViewModel>();
        }

        public async Task<AdminUserEditViewModel?> GetUserByIdAsync(string id)
        {
            var response = await _client.GetAsync($"/api/admin/users/{id}");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<AdminUserEditViewModel>(JsonOptions);
        }

        public async Task<bool> CreateUserAsync(AdminUserCreateViewModel model)
        {
            var response = await _client.PostAsJsonAsync("/api/admin/users", model, JsonOptions);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateUserAsync(string id, AdminUserEditViewModel model)
        {
            var response = await _client.PutAsJsonAsync($"/api/admin/users/{id}", model, JsonOptions);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ActivateUserAsync(string id)
        {
            var response = await _client.PostAsync($"/api/admin/users/{id}/activate", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeactivateUserAsync(string id)
        {
            var response = await _client.PostAsync($"/api/admin/users/{id}/deactivate", null);
            return response.IsSuccessStatusCode;
        }
    }
}
