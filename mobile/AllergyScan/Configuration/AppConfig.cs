using Microsoft.Extensions.Configuration;

namespace AllergyScan
{
    public static class AppConfig
    {
        private static readonly IConfiguration Configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        public static string ApiBaseUrl => Configuration["App:ApiBaseUrl"] ?? "http://localhost:8080/";
        public static string ComputerVisionEndpoint => Configuration["Azure:ComputerVision:Endpoint"] ?? "https://kv-computervision.cognitiveservices.azure.com/";
        public static string ComputerVisionKey => Configuration["Azure:ComputerVision:Key"] ?? "";
        public static string TranslatorEndpoint => Configuration["Azure:Translator:Endpoint"] ?? "https://api.cognitive.microsofttranslator.com";
        public static string TranslatorKey => Configuration["Azure:Translator:Key"] ?? "";
        public static string TranslatorRegion => Configuration["Azure:Translator:Region"] ?? "francecentral";
    }
}
