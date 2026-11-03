using AllergyScan.Models;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AllergyScan.Services
{
    public interface IAllergyDataService
    {
        Task<List<Allergy>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Allergy> CreateAsync(string name, string? nl = null, string? fr = null, string? de = null, string? es = null, CancellationToken cancellationToken = default);
        Task<Allergy> UpdateAsync(int id, string name, CancellationToken cancellationToken = default);
        Task<Allergy> UpdateAsync(int id, string name, ScanFlag flag, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }

    public class AllergyDataService : IAllergyDataService
    {
        private static readonly HttpClient Client = new()
        {
            Timeout = TimeSpan.FromSeconds(60)
        };

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public Task<List<Allergy>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return GetAsync<List<Allergy>>("api/allergies", cancellationToken);
        }

        public Task<Allergy> CreateAsync(string name, string? nl = null, string? fr = null, string? de = null, string? es = null, CancellationToken cancellationToken = default)
        {
            var request = new AllergyRequest
            {
                Name = name,
                Flag = ScanFlag.Contains,
                TranslationNl = nl,
                TranslationFr = fr,
                TranslationDe = de,
                TranslationEs = es
            };
            return PostAsync<AllergyRequest, Allergy>("api/allergies", request, cancellationToken);
        }

        public Task<Allergy> UpdateAsync(int id, string name, CancellationToken cancellationToken = default)
        {
            var request = new AllergyRequest { Name = name };
            return PutAsync<AllergyRequest, Allergy>($"api/allergies/{id}", request, cancellationToken);
        }

        public Task<Allergy> UpdateAsync(int id, string name, ScanFlag flag, CancellationToken cancellationToken = default)
        {
            var request = new AllergyRequest { Name = name, Flag = flag };
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
            return JsonSerializer.Deserialize<T>(json, JsonOptions)!;
        }

        private static async Task<TResponse> PostAsync<TRequest, TResponse>(string relativeUrl, TRequest request, CancellationToken cancellationToken)
        {
            using var content = new StringContent(JsonSerializer.Serialize(request, JsonOptions), System.Text.Encoding.UTF8, "application/json");
            using var response = await Client.PostAsync(BuildUri(relativeUrl), content, cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<TResponse>(json, JsonOptions)!;
        }

        private static async Task<TResponse> PutAsync<TRequest, TResponse>(string relativeUrl, TRequest request, CancellationToken cancellationToken)
        {
            using var content = new StringContent(JsonSerializer.Serialize(request, JsonOptions), System.Text.Encoding.UTF8, "application/json");
            using var response = await Client.PutAsync(BuildUri(relativeUrl), content, cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<TResponse>(json, JsonOptions)!;
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
            public ScanFlag? Flag { get; set; }
            public string? TranslationNl { get; set; }
            public string? TranslationFr { get; set; }
            public string? TranslationDe { get; set; }
            public string? TranslationEs { get; set; }
        }

        private sealed class ProblemDetails
        {
            public string? Title { get; set; }
            public int Status { get; set; }
            public string? Detail { get; set; }
        }
    }
}
