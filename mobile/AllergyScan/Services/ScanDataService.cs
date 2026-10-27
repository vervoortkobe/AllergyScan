using AllergyScan.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace AllergyScan.Services
{
    public interface IScanDataService
    {
        Task<PagedResult<ScanHistory>> GetHistoryAsync(int page, int pageSize, CancellationToken cancellationToken = default);
        Task<ScanHistory> CreateAsync(byte[]? image, string? imageFileName, string? imageContentType, string text, ScanFlag flag, CancellationToken cancellationToken = default);
        Task<ScanHistory> UpdateAsync(int id, string text, ScanFlag flag, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task DeleteAllAsync(CancellationToken cancellationToken = default);
    }

    public class ScanDataService : IScanDataService
    {
        private static readonly HttpClient Client = new()
        {
            Timeout = TimeSpan.FromSeconds(60)
        };

        public Task<PagedResult<ScanHistory>> GetHistoryAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        {
            return GetAsync<PagedResult<ScanHistory>>($"api/scans?page={page}&pageSize={pageSize}", cancellationToken);
        }

        public Task<ScanHistory> CreateAsync(byte[]? image, string? imageFileName, string? imageContentType, string text, ScanFlag flag, CancellationToken cancellationToken = default)
        {
            var content = new MultipartFormDataContent();
            if (image is { Length: > 0 })
            {
                var imageContent = new ByteArrayContent(image);
                imageContent.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(
                    string.IsNullOrWhiteSpace(imageContentType) ? "image/jpeg" : imageContentType);
                content.Add(imageContent, "image", string.IsNullOrWhiteSpace(imageFileName) ? "scan.jpg" : imageFileName);
            }

            content.Add(new StringContent(text, System.Text.Encoding.UTF8, "text/plain"), "text");
            content.Add(new StringContent(flag.ToString(), System.Text.Encoding.UTF8, "text/plain"), "flag");

            return PostContentAsync<ScanHistory>("api/scans", content, cancellationToken);
        }

        public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            return DeleteRequestAsync($"api/scans/{id}", cancellationToken);
        }

        public Task DeleteAllAsync(CancellationToken cancellationToken = default)
        {
            return DeleteRequestAsync("api/scans", cancellationToken);
        }

        public Task<ScanHistory> UpdateAsync(int id, string text, ScanFlag flag, CancellationToken cancellationToken = default)
        {
            var request = new UpdateScanRequest { Text = text, Flag = flag };
            var content = new StringContent(
                JsonSerializer.Serialize(request),
                System.Text.Encoding.UTF8, "application/json");
            return PutContentAsync<ScanHistory>($"api/scans/{id}", content, cancellationToken);
        }

        private static async Task<T> GetAsync<T>(string relativeUrl, CancellationToken cancellationToken)
        {
            using var response = await Client.GetAsync(BuildUri(relativeUrl), cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        private static async Task<TResponse> PostContentAsync<TResponse>(string relativeUrl, HttpContent content, CancellationToken cancellationToken)
        {
            using var response = await Client.PostAsync(BuildUri(relativeUrl), content, cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<TResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        private static async Task<TResponse> PutContentAsync<TResponse>(string relativeUrl, HttpContent content, CancellationToken cancellationToken)
        {
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

        private sealed class ProblemDetails
        {
            public string? Title { get; set; }
            public int Status { get; set; }
            public string? Detail { get; set; }
        }

        private sealed class UpdateScanRequest
        {
            public string? Text { get; set; }
            public ScanFlag Flag { get; set; }
        }
    }
}
