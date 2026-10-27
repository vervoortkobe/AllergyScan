using AllergyScan.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace AllergyScan.Services
{
    public interface IAllergyDataService
    {
        Task<List<Allergy>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Allergy> CreateAsync(string name, CancellationToken cancellationToken = default);
        Task<Allergy> UpdateAsync(int id, string name, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }

    public class AllergyDataService : IAllergyDataService
    {
        private static readonly HttpClient Client = new()
        {
            Timeout = TimeSpan.FromSeconds(60)
        };

        public Task<List<Allergy>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return GetAsync<List<Allergy>>("api/allergies", cancellationToken);
        }

        public Task<Allergy> CreateAsync(string name, CancellationToken cancellationToken = default)
        {
            var request = new AllergyRequest { Name = name };
            return PostAsync<AllergyRequest, Allergy>("api/allergies", request, cancellationToken);
        }

        public Task<Allergy> UpdateAsync(int id, string name, CancellationToken cancellationToken = default)
        {
            var request = new AllergyRequest { Name = name };
            return PutAsync<AllergyRequest, Allergy>($"api/allergies/{id}", request, cancellationToken);
        }

        public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            return DeleteRequestAsync($"api/allergies/{id}", cancellationToken);
        }

        private static async Task<T> GetAsync<T>(string relativeUrl, CancellationToken cancellationToken)
        {
            using var response = await Client.GetAsync(BuildUri(relativeUrl), cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        private static async Task<TResponse> PostAsync<TRequest, TResponse>(string relativeUrl, TRequest request, CancellationToken cancellationToken)
        {
            using var content = new StringContent(JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json");
            using var response = await Client.PostAsync(BuildUri(relativeUrl), content, cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<TResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        private static async Task<TResponse> PutAsync<TRequest, TResponse>(string relativeUrl, TRequest request, CancellationToken cancellationToken)
        {
            using var content = new StringContent(JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json");
            using var response = await Client.PutAsync(BuildUri(relativeUrl), content, cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<TResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        private static async Task DeleteRequestAsync(string relativeUrl, CancellationToken cancellationToken)
        {
            using var response = await Client.DeleteAsync(BuildUri(relativeUrl), cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);
        }

        private static Uri BuildUri(string relativeUrl)
        {
            var baseUrl = AppConfig.ApiBaseUrl;
            return new Uri(new Uri(baseUrl), relativeUrl.TrimStart('/'));
        }

        private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);
                throw new Exception(error?.Detail ?? response.ReasonPhrase);
            }
        }

        private sealed class AllergyRequest
        {
            public string Name { get; set; } = string.Empty;
        }

        private sealed class ProblemDetails
        {
            public string? Title { get; set; }
            public int Status { get; set; }
            public string? Detail { get; set; }
        }
    }
}
