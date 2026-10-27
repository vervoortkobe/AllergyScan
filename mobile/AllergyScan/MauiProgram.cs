using AllergyScan.Services;
using AllergyScan.ViewModels;
using Microsoft.Extensions.Configuration;

namespace AllergyScan
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Add appsettings
            builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            // Shell
            builder.Services.AddSingleton<AppShell>();

            // Services
            builder.Services.AddSingleton<IAllergyDataService, AllergyDataService>();
            builder.Services.AddSingleton<ILanguageDataService, LanguageDataService>();
            builder.Services.AddSingleton<IScanDataService, ScanDataService>();
            builder.Services.AddSingleton<IOCRService, OCRService>();
            builder.Services.AddSingleton<ITranslationService, TranslationService>();
            builder.Services.AddSingleton<IAllergenDetectionService, AllergenDetectionService>();
            builder.Services.AddSingleton<IPhotoService, PhotoService>();
            builder.Services.AddSingleton<IDialogService, DialogService>();
            builder.Services.AddSingleton<INavigationService, NavigationService>();

            // View Models
            builder.Services.AddTransient<AllergiesViewModel>();
            builder.Services.AddTransient<HistoryViewModel>();
            builder.Services.AddTransient<SettingsViewModel>();
            builder.Services.AddTransient<ScanViewModel>();
            builder.Services.AddTransient<ScanDetailsViewModel>();
            builder.Services.AddTransient<SetupViewModel>();

            // Pages
            builder.Services.AddTransient<Views.AllergiesPage>();
            builder.Services.AddTransient<Views.HistoryPage>();
            builder.Services.AddTransient<Views.SettingsPage>();
            builder.Services.AddTransient<Views.ScanPage>();
            builder.Services.AddTransient<Views.ScanDetailsPage>();
            builder.Services.AddTransient<Views.SetupPage>();

            return builder.Build();
        }
    }
}
