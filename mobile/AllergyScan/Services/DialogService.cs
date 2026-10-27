namespace AllergyScan.Services
{
    public interface IDialogService
    {
        Task ShowAlertAsync(string title, string message);
        Task<bool> ConfirmAsync(string title, string message, string accept = "OK", string cancel = "Cancel");
    }

    public class DialogService : IDialogService
    {
        public Task ShowAlertAsync(string title, string message)
        {
            var page = Application.Current?.Windows.FirstOrDefault()?.Page;
            return page?.DisplayAlertAsync(title, message, "OK") ?? Task.CompletedTask;
        }

        public async Task<bool> ConfirmAsync(string title, string message, string accept = "OK", string cancel = "Cancel")
        {
            var page = Application.Current?.Windows.FirstOrDefault()?.Page;
            if (page is null)
            {
                return false;
            }

            return await page.DisplayAlertAsync(title, message, accept, cancel);
        }
    }
}
