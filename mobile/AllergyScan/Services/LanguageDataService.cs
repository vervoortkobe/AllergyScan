using AllergyScan.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace AllergyScan.Services
{
    public interface ILanguageDataService
    {
        Task<List<Language>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Language> GetCurrentAsync(CancellationToken cancellationToken = default);
        Task<Language> SetCurrentAsync(string code, CancellationToken cancellationToken = default);
    }

    public class LanguageDataService : ILanguageDataService
    {
        private static readonly HttpClient Client = new()
        {
            Timeout = TimeSpan.FromSeconds(60)
        };

        public Task<List<Language>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return GetAsync<List<Language>>("api/languages", cancellationToken);
        }

        public Task<Language> GetCurrentAsync(CancellationToken cancellationToken = default)
        {
            return GetAsync<Language>("api/languages/current", cancellationToken);
        }

        public Task<Language> SetCurrentAsync(string code, CancellationToken cancellationToken = default)
        {
            var request = new LanguageRequest { Code = code };
            return PutAsync<LanguageRequest, Language>("api/languages/current", request, cancellationToken);
        }

        private static async Task<T> GetAsync<T>(string relativeUrl, CancellationToken cancellationToken)
        {
            using var response = await Client.GetAsync(BuildUri(relativeUrl), cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        private static async Task<TResponse> PutAsync<TRequest, TResponse>(string relativeUrl, TRequest request, CancellationToken cancellationToken)
        {
            using var content = new StringContent(JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json");
            using var response = await Client.PutAsync(BuildUri(relativeUrl), content, cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<TResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
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

        private sealed class LanguageRequest
        {
            public string Code { get; set; } = string.Empty;
        }

        private sealed class ProblemDetails
        {
            public string? Title { get; set; }
            public int Status { get; set; }
            public string? Detail { get; set; }
        }
    }
}
