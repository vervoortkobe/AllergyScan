using Newtonsoft.Json.Linq;
using System.Text;

namespace AllergyScan.Services
{
    public interface ITranslationService
    {
        Task<TranslationResult> TranslateAsync(string text, string toLanguage);
    }

    public class TranslationResult
    {
        public string Text { get; set; } = string.Empty;
        public string? DetectedLanguage { get; set; }
    }

    public class TranslationService : ITranslationService
    {
        public async Task<TranslationResult> TranslateAsync(string text, string toLanguage)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", AppConfig.TranslatorKey);
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Region", AppConfig.TranslatorRegion);

            var endpoint = $"{AppConfig.TranslatorEndpoint.TrimEnd('/')}/translate?api-version=3.0&to={toLanguage}";

            var body = new object[] { new { Text = text } };
            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync(endpoint, content);
            var jsonResponse = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(
                    response.StatusCode,
                    $"Translation failed: {response.StatusCode} - {response.ReasonPhrase}");
            }

            var translations = JArray.Parse(jsonResponse);
            var firstItem = translations.FirstOrDefault();
            var firstTranslation = firstItem?["translations"]?.FirstOrDefault();

            var detectedLanguage = firstItem?["detectedLanguage"]?["language"]?.ToString()
                ?? firstTranslation?["detectedLanguage"]?["language"]?.ToString();

            var result = new TranslationResult
            {
                Text = firstTranslation?["text"]?.ToString() ?? text,
                DetectedLanguage = detectedLanguage
            };

            return result;
        }
    }
}
