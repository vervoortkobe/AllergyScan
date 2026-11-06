using AllergyScan.Interfaces;
using AllergyScan.Localization;
using AllergyScan.Services;
using AllergyScan.ViewModels;
using AllergyScan.Views;

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

            // Configuration comes from Properties/appsettings.json (embedded in the assembly).
            builder.Services.AddSingleton(AppConfig.Configuration);

            // Shell
            builder.Services.AddTransient<AppShell>();

            // Localization
            builder.Services.AddSingleton<ILocalizationService>(_ => LocalizationService.Instance);

            // Services
            builder.Services.AddSingleton<IAllergyDataService, AllergyDataService>();
            builder.Services.AddSingleton<ILanguageDataService, LanguageDataService>();
            builder.Services.AddSingleton<IScanDataService, ScanDataService>();
            builder.Services.AddSingleton<IOCRService, OCRService>();
            builder.Services.AddSingleton<ITranslationService, TranslationService>();
            builder.Services.AddSingleton<IAllergenDetectionService, AllergenDetectionService>();
            builder.Services.AddSingleton<IHistoryRescanService, HistoryRescanService>();
            builder.Services.AddSingleton<IPhotoService, PhotoService>();
            builder.Services.AddSingleton<IDialogService, DialogService>();
            builder.Services.AddSingleton<INavigationService, NavigationService>();

            // ViewModels
            builder.Services.AddTransient<IAllergiesViewModel, AllergiesViewModel>();
            builder.Services.AddTransient<IHistoryViewModel, HistoryViewModel>();
            builder.Services.AddTransient<ISettingsViewModel, SettingsViewModel>();
            builder.Services.AddTransient<IScanViewModel, ScanViewModel>();
            builder.Services.AddTransient<IScanDetailsViewModel, ScanDetailsViewModel>();
            builder.Services.AddTransient<ISetupLanguageViewModel, SetupLanguageViewModel>();
            builder.Services.AddTransient<ISetupAllergiesViewModel, SetupAllergiesViewModel>();
            builder.Services.AddTransient<IResultViewModel, ResultViewModel>();

            // Views
            builder.Services.AddTransient<AllergiesPage>();
            builder.Services.AddTransient<HistoryPage>();
            builder.Services.AddTransient<SettingsPage>();
            builder.Services.AddTransient<ScanPage>();
            builder.Services.AddTransient<ScanDetailsPage>();
            builder.Services.AddTransient<SetupLanguagePage>();
            builder.Services.AddTransient<SetupAllergiesPage>();
            builder.Services.AddTransient<ResultPage>();

            return builder.Build();
        }
    }
}
