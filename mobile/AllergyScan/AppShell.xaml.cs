using AllergyScan.Services;

namespace AllergyScan
{
    public partial class AppShell : Shell
    {
        private readonly IAllergyDataService allergyDataService;
        private readonly ILanguageDataService languageDataService;
        private readonly IServiceProvider serviceProvider;
        private bool hasCheckedStartup;

        public AppShell(
            IServiceProvider serviceProvider,
            IAllergyDataService allergyDataService,
            ILanguageDataService languageDataService)
        {
            InitializeComponent();
            this.serviceProvider = serviceProvider;
            this.allergyDataService = allergyDataService;
            this.languageDataService = languageDataService;
            RegisterRoutes();
        }

        private void RegisterRoutes()
        {
            // Register routes, ignoring duplicates if they already exist
            TryRegisterRoute("scanDetails", new ServiceRouteFactory<Views.ScanDetailsPage>(serviceProvider));
            TryRegisterRoute("setup", new ServiceRouteFactory<Views.SetupLanguagePage>(serviceProvider));
            TryRegisterRoute("setup-allergies", new ServiceRouteFactory<Views.SetupAllergiesPage>(serviceProvider));
            TryRegisterRoute("result", new ServiceRouteFactory<Views.ResultPage>(serviceProvider));
        }

        private static void TryRegisterRoute(string route, RouteFactory factory)
        {
            try
            {
                Routing.RegisterRoute(route, factory);
            }
            catch (ArgumentException)
            {
                // Route already registered, ignore
            }
        }

        private sealed class ServiceRouteFactory<TPage>(IServiceProvider serviceProvider) : RouteFactory
            where TPage : Element
        {
            public override Element GetOrCreate() => serviceProvider.GetRequiredService<TPage>();

            public override Element GetOrCreate(IServiceProvider services) => (services ?? serviceProvider).GetRequiredService<TPage>();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (hasCheckedStartup)
                return;

            hasCheckedStartup = true;

            try
            {
                await CheckInitialSetupAsync();
            }
            catch (Exception)
            {
                // Log exception silently
            }
        }

        public async Task CheckInitialSetupAsync()
        {
            try
            {
                bool setupDone = false;
                bool langSet = false;
                try
                {
                    setupDone = Preferences.Default.Get("HasCompletedInitialSetup", false);
                    langSet = Preferences.Default.Get("LanguageConfigured", false);
                }
                catch (Exception)
                {
                    // Preferences may not be available
                }

                if (!setupDone || !langSet)
                {
                    try
                    {
                        await GoToAsync("setup");
                    }
                    catch (Exception)
                    {
                        // Navigation failed
                    }
                    return;
                }

                try
                {
                    var allergies = await allergyDataService.GetAllAsync();
                    if (allergies.Count == 0)
                    {
                        try
                        {
                            await GoToAsync("setup");
                        }
                        catch (Exception)
                        {
                            // Navigation failed
                        }
                    }
                }
                catch
                {
                    // If offline, continue without blocking
                }
            }
            catch (Exception)
            {
                // Log exception silently
            }
        }
    }
}
