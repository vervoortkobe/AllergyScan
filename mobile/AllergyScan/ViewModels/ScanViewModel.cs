using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using AllergyScan.Messages;
using AllergyScan.Models;
using AllergyScan.Services;

namespace AllergyScan.ViewModels
{
    public class ScanViewModel : ObservableRecipient, IRecipient<LanguageChangedMessage>
    {
        private readonly IOCRService ocrService;
        private readonly ITranslationService translationService;
        private readonly IAllergenDetectionService detectionService;
        private readonly IAllergyDataService allergyDataService;
        private readonly ILanguageDataService languageDataService;
        private readonly IScanDataService scanDataService;
        private readonly IPhotoService photoService;
        private readonly IDialogService dialogService;

        private byte[]? imageBytes;
        private string imageFileName = "scan.jpg";
        private string imageContentType = "image/jpeg";
        private ImageSource? imageSource;
        private string originalText = string.Empty;
        private string translatedText = string.Empty;
        private string? detectedLanguage;
        private string detectedAllergens = string.Empty;
        private bool hasResult;
        private bool hasAllergens;
        private bool hasImage;
        private bool isBusy;
        private string statusMessage = "Take or choose a photo of an ingredients list.";
        private string currentLanguageName = "Loading…";
        private string currentLanguageCode = "en";
        private ScanFlag flag;

        public ScanViewModel(
            IOCRService ocrService,
            ITranslationService translationService,
            IAllergenDetectionService detectionService,
            IAllergyDataService allergyDataService,
            ILanguageDataService languageDataService,
            IScanDataService scanDataService,
            IPhotoService photoService,
            IDialogService dialogService)
        {
            this.ocrService = ocrService;
            this.translationService = translationService;
            this.detectionService = detectionService;
            this.allergyDataService = allergyDataService;
            this.languageDataService = languageDataService;
            this.scanDataService = scanDataService;
            this.photoService = photoService;
            this.dialogService = dialogService;

            TakePhotoCommand = new AsyncRelayCommand(TakePhotoAsync);
            PickPhotoCommand = new AsyncRelayCommand(PickPhotoAsync);
            ScanCommand = new AsyncRelayCommand(ScanAsync);
            SaveCommand = new AsyncRelayCommand(SaveAsync);
            ResetCommand = new RelayCommand(Reset);

            Messenger.Register<ScanViewModel, LanguageChangedMessage>(
                this,
                static (recipient, message) => recipient.Receive(message));
        }

        public ICommand TakePhotoCommand { get; }

        public ICommand PickPhotoCommand { get; }

        public ICommand ScanCommand { get; }

        public ICommand SaveCommand { get; }

        public ICommand ResetCommand { get; }

        public ImageSource? PreviewImage
        {
            get => imageSource;
            private set => SetProperty(ref imageSource, value);
        }

        public string OriginalText
        {
            get => originalText;
            set => SetProperty(ref originalText, value);
        }

        public string TranslatedText
        {
            get => translatedText;
            private set
            {
                if (SetProperty(ref translatedText, value))
                {
                    OnPropertyChanged(nameof(HasTranslatedText));
                }
            }
        }

        public string? DetectedLanguage
        {
            get => detectedLanguage;
            private set
            {
                if (SetProperty(ref detectedLanguage, value))
                {
                    OnPropertyChanged(nameof(DetectedLanguageText));
                }
            }
        }

        public string DetectedAllergens
        {
            get => detectedAllergens;
            private set
            {
                if (SetProperty(ref detectedAllergens, value))
                {
                    OnPropertyChanged(nameof(FlagDescription));
                }
            }
        }

        public bool HasResult
        {
            get => hasResult;
            private set
            {
                if (SetProperty(ref hasResult, value))
                {
                    OnPropertyChanged(nameof(CanSave));
                }
            }
        }

        public bool HasAllergens
        {
            get => hasAllergens;
            private set
            {
                if (SetProperty(ref hasAllergens, value))
                {
                    OnPropertyChanged(nameof(FlagTitle));
                    OnPropertyChanged(nameof(FlagColor));
                    OnPropertyChanged(nameof(FlagBackgroundColor));
                    OnPropertyChanged(nameof(FlagDescription));
                }
            }
        }

        public bool HasImage
        {
            get => hasImage;
            private set
            {
                if (SetProperty(ref hasImage, value))
                {
                    OnPropertyChanged(nameof(CanScan));
                    OnPropertyChanged(nameof(HasNoImage));
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
                    OnPropertyChanged(nameof(CanScan));
                    OnPropertyChanged(nameof(CanSave));
                }
            }
        }

        public string StatusMessage
        {
            get => statusMessage;
            private set => SetProperty(ref statusMessage, value);
        }

        public string CurrentLanguageName
        {
            get => currentLanguageName;
            private set
            {
                if (SetProperty(ref currentLanguageName, value))
                {
                    OnPropertyChanged(nameof(TranslationTargetText));
                }
            }
        }

        public string CurrentLanguageCode
        {
            get => currentLanguageCode;
            private set => SetProperty(ref currentLanguageCode, value);
        }

        public ScanFlag Flag
        {
            get => flag;
            private set => SetProperty(ref flag, value);
        }

        public bool CanScan => HasImage && !IsBusy;

        public bool HasNoImage => !HasImage;

        public bool CanSave => HasResult && !IsBusy;

        public bool HasTranslatedText => !string.IsNullOrWhiteSpace(TranslatedText);

        public string TranslationTargetText => $"Translate to: {CurrentLanguageName}";

        public string DetectedLanguageText => string.IsNullOrWhiteSpace(DetectedLanguage)
            ? string.Empty
            : $"Detected language: {DetectedLanguage.ToUpperInvariant()}";

        public string FlagTitle => HasAllergens
            ? "⚠ Allergen detected"
            : "✓ No allergens detected";

        public Color FlagColor => HasAllergens
            ? Color.FromArgb("#B42318")
            : Color.FromArgb("#067647");

        public Color FlagBackgroundColor => HasAllergens
            ? Color.FromArgb("#FEE4E2")
            : Color.FromArgb("#D1FADF");

        public string FlagDescription => HasAllergens
            ? $"This product may contain: {DetectedAllergens}"
            : "None of your allergies were found in the ingredient text.";

        public void Receive(LanguageChangedMessage message)
        {
            CurrentLanguageCode = message.Value.Code;
            CurrentLanguageName = message.Value.Name;
        }

        public async Task InitializeAsync()
        {
            try
            {
                var language = await languageDataService.GetCurrentAsync();
                CurrentLanguageCode = language.Code;
                CurrentLanguageName = language.Name;
            }
            catch (Exception)
            {
                CurrentLanguageName = "English (API offline)";
                CurrentLanguageCode = "en";
            }
        }

        private async Task TakePhotoAsync()
        {
            try
            {
                var photo = await photoService.CaptureAsync();
                ApplyPhoto(photo);
            }
            catch (FeatureNotSupportedException)
            {
                await dialogService.ShowAlertAsync(
                    "Camera not available",
                    "This device does not support taking photos. Choose a photo from your gallery instead.");
            }
            catch (PermissionException)
            {
                await dialogService.ShowAlertAsync(
                    "Permission needed",
                    "Allow camera access to photograph an ingredients list.");
            }
            catch (Exception exception)
            {
                await dialogService.ShowAlertAsync("Could not take photo", exception.Message);
            }
        }

        private async Task PickPhotoAsync()
        {
            try
            {
                var photo = await photoService.PickAsync();
                ApplyPhoto(photo);
            }
            catch (PermissionException)
            {
                await dialogService.ShowAlertAsync(
                    "Permission needed",
                    "Allow photo access to choose an ingredients list from your gallery.");
            }
            catch (Exception exception)
            {
                await dialogService.ShowAlertAsync("Could not open photos", exception.Message);
            }
        }

        private void ApplyPhoto(PickedPhoto? photo)
        {
            if (photo is null)
            {
                return;
            }

            imageBytes = photo.Data;
            imageFileName = photo.FileName;
            imageContentType = photo.ContentType;

            PreviewImage = Microsoft.Maui.Controls.ImageSource.FromStream(() => new MemoryStream(photo.Data));
            HasImage = true;
            ClearResult();
            StatusMessage = "Photo ready. Tap “Scan ingredients” to read and translate it.";
        }

        private async Task ScanAsync()
        {
            if (!CanScan || imageBytes is null)
            {
                return;
            }

            IsBusy = true;
            ClearResult();
            StatusMessage = "Reading the ingredients with Azure Computer Vision…";

            try
            {
                using var imageStream = new MemoryStream(imageBytes);
                var text = (await ocrService.ReadTextAsync(imageStream)).Trim();

                if (string.IsNullOrWhiteSpace(text))
                {
                    StatusMessage = "No text was found in the photo.";
                    await dialogService.ShowAlertAsync(
                        "No text found",
                        "Computer Vision could not read any text. Try a sharper photo with the ingredients list clearly visible.");
                    return;
                }

                StatusMessage = $"Translating to {CurrentLanguageName}…";
                var translation = await translationService.TranslateAsync(text, CurrentLanguageCode);

                StatusMessage = "Comparing the ingredients with your allergy profile…";
                var allergies = await allergyDataService.GetAllAsync();
                var matches = detectionService.Detect(text, translation.Text, allergies);

                OriginalText = text;
                TranslatedText = translation.Text;
                DetectedLanguage = translation.DetectedLanguage;
                HasAllergens = matches.Count > 0;
                Flag = HasAllergens ? ScanFlag.AllergenDetected : ScanFlag.NoAllergen;
                DetectedAllergens = string.Join(", ", matches.Select(match => match.Name));
                HasResult = true;
                StatusMessage = "Scan complete. Review the result and save it to your history.";
            }
            catch (ApiException exception)
            {
                StatusMessage = exception.Message;
                await dialogService.ShowAlertAsync("Scan failed", exception.Message);
            }
            catch (Exception exception)
            {
                StatusMessage = "The scan failed.";
                await dialogService.ShowAlertAsync("Scan failed", exception.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task SaveAsync()
        {
            if (!CanSave || imageBytes is null)
            {
                return;
            }

            IsBusy = true;
            StatusMessage = "Saving the scan to your history…";

            try
            {
                await scanDataService.CreateAsync(
                    imageBytes,
                    imageFileName,
                    imageContentType,
                    OriginalText,
                    Flag);

                WeakReferenceMessenger.Default.Send(new RefreshScansMessage());
                StatusMessage = "Saved to your history.";
            }
            catch (ApiException exception)
            {
                StatusMessage = exception.Message;
                await dialogService.ShowAlertAsync("Could not save scan", exception.Message);
            }
            catch (Exception exception)
            {
                StatusMessage = "Could not save the scan.";
                await dialogService.ShowAlertAsync("Could not save scan", exception.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void Reset()
        {
            imageBytes = null;
            PreviewImage = null;
            HasImage = false;
            ClearResult();
            StatusMessage = "Take or choose a photo of an ingredients list.";
        }

        private void ClearResult()
        {
            OriginalText = string.Empty;
            TranslatedText = string.Empty;
            DetectedLanguage = null;
            DetectedAllergens = string.Empty;
            HasAllergens = false;
            Flag = ScanFlag.NoAllergen;
            HasResult = false;
        }
    }
}
