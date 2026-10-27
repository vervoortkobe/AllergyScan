using Newtonsoft.Json.Linq;
using System.Text;

namespace AllergyScan.Services
{
    public interface IOCRService
    {
        Task<string> ReadTextAsync(Stream imageStream);
    }

    public class OCRService : IOCRService
    {
        public async Task<string> ReadTextAsync(Stream imageStream)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", AppConfig.ComputerVisionKey);

            var endpoint = $"{AppConfig.ComputerVisionEndpoint.TrimEnd('/')}/vision/v3.2/read/analyze?readingOrder=natural";

            using var memoryStream = new MemoryStream();
            await imageStream.CopyToAsync(memoryStream);
            var imageBytes = memoryStream.ToArray();

            var content = new ByteArrayContent(imageBytes);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

            var response = await client.PostAsync(endpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(
                    response.StatusCode,
                    $"OCR failed: {response.StatusCode} - {response.ReasonPhrase}");
            }

            var operationLocation = response.Headers.GetValues("Operation-Location").FirstOrDefault();

            if (string.IsNullOrEmpty(operationLocation))
            {
                throw new ApiException(
                    System.Net.HttpStatusCode.InternalServerError,
                    "No Operation-Location header returned");
            }

            string operationUrl = operationLocation;
            if (!operationUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                operationUrl = $"{AppConfig.ComputerVisionEndpoint.TrimEnd('/')}/{operationUrl.TrimStart('/')}";
            }

            int retries = 0;
            const int maxRetries = 20;
            const int delayMs = 500;

            while (retries < maxRetries)
            {
                await Task.Delay(delayMs);

                var getResponse = await client.GetAsync(operationUrl);
                var jsonResponse = await getResponse.Content.ReadAsStringAsync();

                var json = JObject.Parse(jsonResponse);
                var status = json["status"]?.ToString();

                if (string.Equals(status, "Succeeded", StringComparison.OrdinalIgnoreCase))
                {
                    var readResults = (json["analyzeResult"]?["readResults"] ?? json["recognitionResults"]) as JArray;
                    if (readResults != null)
                    {
                        var text = new StringBuilder();
                        foreach (var page in readResults)
                        {
                            var lines = page["lines"] as JArray;
                            if (lines != null)
                            {
                                foreach (var line in lines)
                                {
                                    var lineText = line["text"]?.ToString();
                                    if (!string.IsNullOrWhiteSpace(lineText))
                                    {
                                        text.AppendLine(lineText);
                                    }
                                }
                            }
                        }
                        return text.ToString().Trim();
                    }
                    return string.Empty;
                }
                else if (string.Equals(status, "Failed", StringComparison.OrdinalIgnoreCase))
                {
                    throw new ApiException(
                        System.Net.HttpStatusCode.InternalServerError,
                        "OCR operation failed");
                }

                retries++;
            }

            throw new ApiException(
                System.Net.HttpStatusCode.RequestTimeout,
                "OCR operation timed out");
        }
    }
}
