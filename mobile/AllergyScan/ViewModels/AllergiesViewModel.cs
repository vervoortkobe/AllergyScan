using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AllergyScan.Models;
using AllergyScan.Services;

namespace AllergyScan.ViewModels
{
    public class AllergiesViewModel : ObservableObject
    {
        private readonly IAllergyDataService allergyDataService;
        private readonly IDialogService dialogService;

        private ObservableCollection<Allergy> allergies = new();
        private Allergy? selectedAllergy;
        private string newAllergyName = string.Empty;
        private bool isBusy;
        private string statusMessage = string.Empty;

        public AllergiesViewModel(
            IAllergyDataService allergyDataService,
            IDialogService dialogService)
        {
            this.allergyDataService = allergyDataService;
            this.dialogService = dialogService;

            AddCommand = new AsyncRelayCommand(AddAsync);
            DeleteSelectedCommand = new AsyncRelayCommand(DeleteSelectedAsync);
        }

        public ObservableCollection<Allergy> Allergies
        {
            get => allergies;
            set
            {
                if (SetProperty(ref allergies, value))
                {
                    OnPropertyChanged(nameof(CountText));
                }
            }
        }

        public Allergy? SelectedAllergy
        {
            get => selectedAllergy;
            set
            {
                if (SetProperty(ref selectedAllergy, value))
                {
                    OnPropertyChanged(nameof(CanDeleteSelected));
                }
            }
        }

        public string NewAllergyName
        {
            get => newAllergyName;
            set
            {
                if (SetProperty(ref newAllergyName, value))
                {
                    OnPropertyChanged(nameof(CanAdd));
                }
            }
        }

        public bool IsBusy
        {
            get => isBusy;
            private set
            {
                if (SetProperty(ref isBusy, value))
                {
                    OnPropertyChanged(nameof(CanAdd));
                    OnPropertyChanged(nameof(CanDeleteSelected));
                }
            }
        }

        public string StatusMessage
        {
            get => statusMessage;
            private set
            {
                if (SetProperty(ref statusMessage, value))
                {
                    OnPropertyChanged(nameof(HasStatusMessage));
                }
            }
        }

        public string CountText => Allergies.Count switch
        {
            0 => "0 allergies in your profile",
            1 => "1 allergy in your profile",
            var count => $"{count} allergies in your profile"
        };

        public bool CanAdd => !IsBusy && !string.IsNullOrWhiteSpace(NewAllergyName);
        public bool CanDeleteSelected => !IsBusy && SelectedAllergy is not null;

        public bool HasStatusMessage => !string.IsNullOrWhiteSpace(StatusMessage);

        public ICommand AddCommand { get; }
        public ICommand DeleteSelectedCommand { get; }

        public async Task InitializeAsync()
        {
            try
            {
                var result = await allergyDataService.GetAllAsync();
                Allergies = new ObservableCollection<Allergy>(result.OrderBy(a => a.Name));
            }
            catch (Exception exception)
            {
                StatusMessage = exception.Message;
            }
        }

        private async Task AddAsync()
        {
            var name = NewAllergyName.Trim();
            if (string.IsNullOrEmpty(name) || IsBusy)
                return;

            IsBusy = true;
            try
            {
                var allergy = await allergyDataService.CreateAsync(name);
                Allergies.Add(allergy);
                OnPropertyChanged(nameof(CountText));
                NewAllergyName = string.Empty;
                SelectedAllergy = allergy;
                StatusMessage = $"{allergy.Name} added to allergies.";
            }
            catch (Exception exception)
            {
                StatusMessage = exception.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task DeleteSelectedAsync()
        {
            if (SelectedAllergy is null || IsBusy)
                return;

            var allergy = SelectedAllergy;
            IsBusy = true;
            try
            {
                await allergyDataService.DeleteAsync(allergy.Id);
                Allergies.Remove(allergy);
                OnPropertyChanged(nameof(CountText));
                SelectedAllergy = null;
                StatusMessage = $"{allergy.Name} removed from allergies.";
            }
            catch (Exception exception)
            {
                StatusMessage = exception.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
