using Microsoft.Extensions.Configuration;

namespace AllergyScan
{
    /// <summary>
    /// Single source of configuration for the app. The values live in
    /// Properties/appsettings.json, which is embedded into the assembly so it is
    /// available on every platform (Android, iOS and Windows).
    /// </summary>
    public static class AppConfig
    {
        private const string EmbeddedResourceName = "AllergyScan.appsettings.json";

        /// <summary>
        /// The loaded configuration. Embedded settings are loaded first so that a
        /// local appsettings.json next to the executable can override them during
        /// development. Environment variables win over both.
        /// </summary>
        public static IConfiguration Configuration { get; } = BuildConfiguration();

        public static string ApiBaseUrl => Configuration["App:ApiBaseUrl"] ?? "http://localhost:8080/";

        public static string ComputerVisionEndpoint =>
            Configuration["Azure:ComputerVision:Endpoint"] ?? "https://kv-computervision.cognitiveservices.azure.com/";

        public static string ComputerVisionKey =>
            Configuration["Azure:ComputerVision:Key"] ?? string.Empty;

        public static string TranslatorEndpoint =>
            Configuration["Azure:Translator:Endpoint"] ?? "https://api.cognitive.microsofttranslator.com";

        public static string TranslatorKey =>
            Configuration["Azure:Translator:Key"] ?? string.Empty;

        public static string TranslatorRegion =>
            Configuration["Azure:Translator:Region"] ?? "francecentral";

        private static IConfiguration BuildConfiguration()
        {
            var builder = new ConfigurationBuilder();

            var assembly = typeof(AppConfig).Assembly;

            // Build inside the using block: the JSON stream is read when the
            // provider is built, so it must still be open at that point.
            using (var stream = assembly.GetManifestResourceStream(EmbeddedResourceName))
            {
                if (stream is not null)
                {
                    builder.AddJsonStream(stream);
                }

                var baseDirectory = AppContext.BaseDirectory;

                foreach (var relativePath in new[]
                         {
                             "appsettings.json",
                             Path.Combine("Properties", "appsettings.json")
                         })
                {
                    var path = Path.Combine(baseDirectory, relativePath);

                    if (File.Exists(path))
                    {
                        builder.AddJsonFile(path, optional: true, reloadOnChange: true);
                    }
                }

                builder.AddEnvironmentVariables();

                return builder.Build();
            }
        }
    }
}
