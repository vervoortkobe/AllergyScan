using System.Collections.Generic;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using AllergyScan.Localization;
using AllergyScan.Messages;
using AllergyScan.Models;
using AllergyScan.Services;
using SkiaSharp;

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
        private bool hasMayContain;
        private bool hasImage;
        private bool isBusy;
        private string statusMessage = string.Empty;
        private string currentLanguageName = string.Empty;
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

            StatusMessage = T("Scan.Status.Ready");
            CurrentLanguageName = T("Common.Loading");

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
            set
            {
                if (SetProperty(ref originalText, value))
                {
                    OnPropertyChanged(nameof(OriginalTextPreview));
                }
            }
        }

        public string OriginalTextPreview => originalText.Length <= 200
            ? originalText
            : originalText[..200] + "…";

        public string TranslatedText
        {
            get => translatedText;
            private set
            {
                if (SetProperty(ref translatedText, value))
                {
                    OnPropertyChanged(nameof(HasTranslatedText));
                    OnPropertyChanged(nameof(TranslatedTextPreview));
                }
            }
        }

        public string TranslatedTextPreview => translatedText.Length <= 200
            ? translatedText
            : translatedText[..200] + "…";

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

        public bool HasMayContain
        {
            get => hasMayContain;
            private set
            {
                if (SetProperty(ref hasMayContain, value))
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
            private set
            {
                if (SetProperty(ref flag, value))
                {
                    OnPropertyChanged(nameof(FlagTitle));
                    OnPropertyChanged(nameof(FlagColor));
                    OnPropertyChanged(nameof(FlagBackgroundColor));
                    OnPropertyChanged(nameof(FlagDescription));
                    OnPropertyChanged(nameof(HasAllergens));
                }
            }
        }

        public bool CanScan => HasImage && !IsBusy;

        public bool HasNoImage => !HasImage;

        public bool CanSave => HasResult && !IsBusy;

        public bool HasTranslatedText => !string.IsNullOrWhiteSpace(TranslatedText);

        public string TranslationTargetText => string.Format(T("Scan.TranslateTo"), CurrentLanguageName);

        public string DetectedLanguageText => string.IsNullOrWhiteSpace(DetectedLanguage)
            ? string.Empty
            : string.Format(T("Scan.DetectedLanguage"), DetectedLanguage.ToUpperInvariant());

        public string FlagTitle => Flag switch
        {
            ScanFlag.Contains => T("Flag.Contains"),
            ScanFlag.MayContain => T("Flag.MayContain"),
            ScanFlag.MayContainTraces => T("Flag.MayContainTraces"),
            _ => T("Flag.None")
        };

        public Color FlagColor => Flag switch
        {
            ScanFlag.Contains => AllergyScan.Services.FlagColors.TextColor,
            ScanFlag.MayContain => AllergyScan.Services.FlagColors.TextColor,
            ScanFlag.MayContainTraces => AllergyScan.Services.FlagColors.TextColor,
            _ => AllergyScan.Services.FlagColors.TextColor
        };

        public Color FlagBackgroundColor => Flag switch
        {
            ScanFlag.Contains => AllergyScan.Services.FlagColors.ContainsBackground,
            ScanFlag.MayContain => AllergyScan.Services.FlagColors.MayContainBackground,
            ScanFlag.MayContainTraces => AllergyScan.Services.FlagColors.MayContainTracesBackground,
            _ => AllergyScan.Services.FlagColors.SafeBackground
        };

        public string FlagDescription => Flag switch
        {
            ScanFlag.Contains => string.Format(T("Flag.ContainsDescription"), DetectedAllergens),
            ScanFlag.MayContain => string.Format(T("Flag.MayContainDescription"), DetectedAllergens),
            ScanFlag.MayContainTraces => string.Format(T("Flag.MayContainTracesDescription"), DetectedAllergens),
            _ => T("Flag.NoneDescription")
        };

        private static string T(string key) => LocalizationService.Instance[key];

        public void Receive(LanguageChangedMessage message)
        {
            CurrentLanguageCode = message.Value.Code;
            CurrentLanguageName = LocalizationHelper.LanguageName(message.Value.Code, message.Value.Name);
            OnPropertyChanged(nameof(TranslationTargetText));
        }

        public async Task InitializeAsync()
        {
            try
            {
                var language = await languageDataService.GetCurrentAsync();
                CurrentLanguageCode = language.Code;
                CurrentLanguageName = LocalizationHelper.LanguageName(language.Code, language.Name);
            }
            catch (Exception)
            {
                CurrentLanguageName = T("Language.Offline");
                CurrentLanguageCode = "en";
            }
        }

        public void ResetResult()
        {
            imageBytes = null;
            imageSource = null;
            originalText = string.Empty;
            translatedText = string.Empty;
            detectedLanguage = null;
            detectedAllergens = string.Empty;
            hasResult = false;
            hasAllergens = false;
            hasMayContain = false;
            hasImage = false;
            flag = ScanFlag.NoAllergen;
            statusMessage = string.Empty;

            OnPropertyChanged(nameof(ImageSource));
            OnPropertyChanged(nameof(HasImage));
            OnPropertyChanged(nameof(HasResult));
            OnPropertyChanged(nameof(HasAllergens));
            OnPropertyChanged(nameof(HasMayContain));
            OnPropertyChanged(nameof(OriginalText));
            OnPropertyChanged(nameof(TranslatedText));
            OnPropertyChanged(nameof(DetectedLanguage));
            OnPropertyChanged(nameof(DetectedAllergens));
            OnPropertyChanged(nameof(Flag));
            OnPropertyChanged(nameof(FlagTitle));
            OnPropertyChanged(nameof(FlagColor));
            OnPropertyChanged(nameof(FlagBackgroundColor));
            OnPropertyChanged(nameof(FlagDescription));
            OnPropertyChanged(nameof(StatusMessage));
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
                    T("Scan.Dialog.CameraUnavailable.Title"),
                    T("Scan.Dialog.CameraUnavailable.Message"));
            }
            catch (PermissionException)
            {
                await dialogService.ShowAlertAsync(
                    T("Scan.Dialog.Permission.Title"),
                    T("Scan.Dialog.CameraPermission.Message"));
            }
            catch (Exception exception)
            {
                await dialogService.ShowAlertAsync(T("Scan.Dialog.TakePhotoFailed.Title"), exception.Message);
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
                    T("Scan.Dialog.Permission.Title"),
                    T("Scan.Dialog.PhotoPermission.Message"));
            }
            catch (Exception exception)
            {
                await dialogService.ShowAlertAsync(T("Scan.Dialog.PickPhotoFailed.Title"), exception.Message);
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

            PreviewImage = ImageSource.FromStream(() => new MemoryStream(photo.Data));
            HasImage = true;
            ClearResult();
            StatusMessage = T("Scan.Status.PhotoReady");
        }

        private int savedScanId;

        private async Task ScanAsync()
        {
            if (!CanScan || imageBytes is null)
            {
                return;
            }

            IsBusy = true;
            ClearResult();
            StatusMessage = T("Scan.Status.Reading");

            try
            {
                using var imageStream = new MemoryStream(imageBytes);
                using var resized = ResizeImageIfNeeded(imageStream);
                var text = (await ocrService.ReadTextAsync(resized)).Trim();

                if (string.IsNullOrWhiteSpace(text))
                {
                    StatusMessage = T("Scan.Status.NoText");
                    await dialogService.ShowAlertAsync(
                        T("Scan.Dialog.NoText.Title"),
                        T("Scan.Dialog.NoText.Message"));
                    return;
                }

                StatusMessage = string.Format(T("Scan.Status.Translating"), CurrentLanguageName);
                var translation = await translationService.TranslateAsync(text, CurrentLanguageCode);
                var detected = translation.DetectedLanguage ?? CurrentLanguageCode;
                var needsTranslation = !string.Equals(detected, CurrentLanguageCode, StringComparison.OrdinalIgnoreCase);

                var translatedText = needsTranslation ? translation.Text : text;

                StatusMessage = needsTranslation
                    ? string.Format(T("Scan.Status.Translated"), CurrentLanguageName)
                    : string.Format(T("Scan.Status.AlreadyInLanguage"), CurrentLanguageName);

                StatusMessage = T("Scan.Status.Comparing");
                var allergies = await allergyDataService.GetAllAsync();
                var allMatches = detectionService.Detect(text, translatedText, allergies);

                OriginalText = text;
                TranslatedText = translatedText;
                DetectedLanguage = detected;

                var allMatchesList = allMatches.ToList();
                HasAllergens = allMatchesList.Count > 0;
                HasMayContain = allMatchesList.Any(m => m.Flag == ScanFlag.MayContain || m.Flag == ScanFlag.MayContainTraces);

                var maxFlag = allMatchesList.Count > 0
                    ? allMatchesList.Max(m => m.Flag)
                    : ScanFlag.NoAllergen;

                Flag = maxFlag;
                DetectedAllergens = string.Join(
                    ", ",
                    allMatchesList
                        .Select(m => LocalizationHelper.AllergenName(m.Allergy.Name))
                        .Distinct(StringComparer.OrdinalIgnoreCase));
                HasResult = true;
                StatusMessage = T("Scan.Status.Saving");
                await SaveAsync();

                await Shell.Current.GoToAsync($"result?flag={(int)Flag}&allergens={Uri.EscapeDataString(DetectedAllergens ?? string.Empty)}&scanId={savedScanId}");
            }
            catch (ApiException exception)
            {
                StatusMessage = exception.Message;
                await dialogService.ShowAlertAsync(T("Scan.Dialog.ScanFailed.Title"), exception.Message);
            }
            catch (Exception exception)
            {
                StatusMessage = T("Scan.Status.Failed");
                await dialogService.ShowAlertAsync(T("Scan.Dialog.ScanFailed.Title"), exception.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task SaveAsync()
        {
            if (imageBytes is null)
            {
                return;
            }

            IsBusy = true;
            StatusMessage = T("Scan.Status.Saving");

            try
            {
                var saved = await scanDataService.CreateAsync(
                    imageBytes,
                    imageFileName,
                    imageContentType,
                    OriginalText,
                    Flag);
                savedScanId = saved.Id;

                WeakReferenceMessenger.Default.Send(new RefreshScansMessage());
                StatusMessage = T("Scan.Status.Saved");
            }
            catch (ApiException exception)
            {
                StatusMessage = exception.Message;
                await dialogService.ShowAlertAsync(T("Scan.Dialog.SaveFailed.Title"), exception.Message);
            }
            catch (Exception exception)
            {
                StatusMessage = T("Scan.Status.SaveFailed");
                await dialogService.ShowAlertAsync(T("Scan.Dialog.SaveFailed.Title"), exception.Message);
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
            StatusMessage = T("Scan.Status.Ready");
        }

        private void ClearResult()
        {
            OriginalText = string.Empty;
            TranslatedText = string.Empty;
            DetectedLanguage = null;
            DetectedAllergens = string.Empty;
            HasAllergens = false;
            HasMayContain = false;
            Flag = ScanFlag.NoAllergen;
            HasResult = false;
        }

        private Stream ResizeImageIfNeeded(Stream input)
        {
            const long maxBytes = 3_800_000;
            const int maxDimension = 1600;

            input.Position = 0;
            if (input.Length <= maxBytes)
            {
                return input;
            }

            using var original = SKBitmap.Decode(input);
            if (original is null)
            {
                input.Position = 0;
                return input;
            }

            float scale = Math.Min(1f, maxDimension / (float)Math.Max(original.Width, original.Height));
            int newWidth = (int)(original.Width * scale);
            int newHeight = (int)(original.Height * scale);

            using var resized = original.Resize(new SKImageInfo(newWidth, newHeight), new SKSamplingOptions(SKFilterMode.Linear));
            using var image = SKImage.FromBitmap(resized);
            using var data = image.Encode(SKEncodedImageFormat.Jpeg, 85);

            var output = new MemoryStream();
            data.SaveTo(output);
            output.Position = 0;
            return output;
        }
    }
}
