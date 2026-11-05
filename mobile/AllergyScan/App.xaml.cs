namespace AllergyScan
{
    public partial class App : Application
    {
        private readonly IServiceProvider services;

        public App(IServiceProvider services)
        {
            InitializeComponent();
            this.services = services;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(services.GetRequiredService<AppShell>());
        }

        /// <summary>
        /// Rebuilds the Shell so every page is recreated in the currently selected
        /// language. Optionally navigates to a tab route afterwards.
        /// </summary>
        public void ReloadShell(string? route = null)
        {
            var shell = services.GetRequiredService<AppShell>();

            if (Windows.Count > 0)
            {
                Windows[0].Page = shell;
            }

            if (string.IsNullOrWhiteSpace(route))
            {
                return;
            }

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await shell.GoToAsync(route);
                }
                catch
                {

                    // The shell is still starting up; the default tab is fine.
                }
            });
        }
    }
}
