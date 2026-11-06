using AllergyScan.Models;
using System.Windows.Input;

namespace AllergyScan.Interfaces
{
    public interface IScanViewModel
    {
        ImageSource? PreviewImage { get; }
        string OriginalText { get; set; }
        string OriginalTextPreview { get; }
        string TranslatedText { get; }
        string TranslatedTextPreview { get; }
        string? DetectedLanguage { get; }
        string DetectedAllergens { get; }
        bool HasResult { get; }
        bool HasAllergens { get; }
        bool HasMayContain { get; }
        bool HasImage { get; }
        bool IsBusy { get; }
        string StatusMessage { get; }
        string CurrentLanguageName { get; }
        string CurrentLanguageCode { get; }
        ScanFlag Flag { get; }
        bool CanScan { get; }
        bool HasNoImage { get; }
        bool CanSave { get; }
        bool HasTranslatedText { get; }
        string TranslationTargetText { get; }
        string DetectedLanguageText { get; }
        string FlagTitle { get; }
        Color FlagColor { get; }
        Color FlagBackgroundColor { get; }
        string FlagDescription { get; }

        ICommand TakePhotoCommand { get; }
        ICommand PickPhotoCommand { get; }
        ICommand ScanCommand { get; }
        ICommand SaveCommand { get; }
        ICommand ResetCommand { get; }

        Task InitializeAsync();
    }
}
