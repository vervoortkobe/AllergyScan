using AllergyScan.Services;

namespace AllergyScan
{
    public partial class AppShell : Shell
    {
        private readonly IAllergyDataService? allergyDataService;
        private readonly ILanguageDataService? languageDataService;
        private bool hasCheckedStartup;

        public AppShell()
        {
            InitializeComponent();
            RegisterRoutes();
        }

        public AppShell(
            IAllergyDataService allergyDataService,
            ILanguageDataService languageDataService)
        {
            InitializeComponent();
            this.allergyDataService = allergyDataService;
            this.languageDataService = languageDataService;
            RegisterRoutes();
        }

        private void RegisterRoutes()
        {
            Routing.RegisterRoute("scanDetails", typeof(Views.ScanDetailsPage));
            Routing.RegisterRoute("setup", typeof(Views.SetupPage));
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (hasCheckedStartup)
                return;

            hasCheckedStartup = true;
            await CheckInitialSetupAsync();
        }

        public async Task CheckInitialSetupAsync()
        {
            bool setupDone = Preferences.Default.Get("HasCompletedInitialSetup", false);
            bool langSet = Preferences.Default.Get("LanguageConfigured", false);

            if (!setupDone || !langSet)
            {
                await GoToAsync("setup");
                return;
            }

            if (allergyDataService is not null)
            {
                try
                {
                    var allergies = await allergyDataService.GetAllAsync();
                    if (allergies.Count == 0)
                    {
                        await GoToAsync("setup");
                    }
                }
                catch
                {
                    // If offline, continue without blocking
                }
            }
        }
    }
}
