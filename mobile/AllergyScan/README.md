# AllergyScan MAUI app

.NET MAUI client (`net10.0-android`, `net10.0-ios`, `net10.0-maccatalyst`,
`net10.0-windows`) built with the MVVM Community Toolkit. It runs the complete scan
pipeline: photo -> Azure OCR -> Azure Translator -> allergen check -> save to the
WebAPI.

## Screens

| Tab | ViewModel | What it does |
| --- | --- | --- |
| Setup | `SetupViewModel` | Initial startup wizard when no language/allergens are set yet |
| Scan | `ScanViewModel` | Take/choose a photo, run OCR + translation + allergen check, save the scan |
| History | `HistoryViewModel` | Paged list of saved scans, open one, clear the history |
| Allergies | `AllergiesViewModel` | Add and delete the allergies in your profile |
| Settings | `SettingsViewModel` | Choose the translation language, see the API URL |
| Scan details | `ScanDetailsViewModel` | Correct the OCR text, change the flag, delete the scan |

## How it fits together

The project follows the same MVVM structure as the course examples
(*FriendSportConnect*): a page binds to one view model, view models talk to data
services, and view models notify each other with messages.

```
Views/          XAML pages, x:DataType bound to a view model
ViewModels/     ObservableObject / ObservableRecipient with ICommand properties
Models/         Allergy, Language, ScanHistory, ScanFlag, PagedResult
Services/       Data services (WebAPI), Azure services, platform helpers
Messages/       ScanSelectedMessage, RefreshScansMessage, RefreshAllergiesMessage, LanguageChangedMessage
Configuration/  AppConfig (API URL and Azure keys)
```

- **Data services** (`AllergyDataService`, `LanguageDataService`, `ScanDataService`)
  derive from `ApiService`, which wraps `HttpClient`, handles camelCase JSON,
  string enums and converts `application/problem+json` errors into `ApiException`
  with a readable message. This mirrors `ApiService<T>` from the course example.
- **Messages** use `WeakReferenceMessenger`. Saving a scan sends
  `RefreshScansMessage`, the History view model receives it and reloads. Opening a
  scan sends `ScanSelectedMessage` to `ScanDetailsViewModel`. Saving a language
  sends `LanguageChangedMessage` to `ScanViewModel`.
- **Navigation** goes through `INavigationService`, which resolves
  `ScanDetailsPage` from the DI container and pushes it.
- **Azure services** live in `ComputerVisionService` (`Azure.AI.Vision.ImageAnalysis`)
  and `TranslationService` (Azure Translator REST API). `AllergenDetectionService`
  compares the OCR text and the translation with the allergy profile and knows
  allergen names in English, Dutch, French, German and Spanish.

Dependency injection is configured in `MauiProgram.cs`; pages, view models and
services are registered there.

## Configuration

### Backend URL

`Configuration/AppConfig.cs`:

```csharp
private const string Host = "localhost"; // Android build uses 10.0.2.2
private const int Port = 8080;           // docker compose maps 8080
```

- `docker compose up` -> keep `8080`.
- `dotnet run --project AllergyScan.WebAPI` -> use `5236`.
- Physical phone -> use your machine's LAN IP, for example `192.168.0.25`.

`10.0.2.2` is the alias the Android emulator uses for the host machine.
Cleartext HTTP is enabled on Android (`usesCleartextTraffic="true"`) and local
networking is allowed on iOS/MacCatalyst, because the API runs over plain HTTP in
development.

### Azure keys

`Configuration/AppConfig.cs` reads all keys from `appsettings.json` (Computer Vision
and Translator endpoint, key and region). These are the course subscription keys also
used in the OCRFifa and EchoTranslate examples. In a real app they would not be in
source control; the app calls the Azure services directly from the device.

No keys are needed in the app for the backend, because the backend does not call
Azure.

## Run

```bash
# Windows
dotnet build -t:Run -f net10.0-windows10.0.19041.0

# Android emulator (start an emulator first)
dotnet build -t:Run -f net10.0-android

# iOS / MacCatalyst (on macOS)
dotnet build -t:Run -f net10.0-ios
```

## Try it without a camera

The scan needs a photo, but the rest of the app works with any text:

1. Start the backend (`docker compose up --build`).
2. Open **Allergies** and add `milk`, `egg`, `soy`.
3. Open **Settings** and pick a translation language.
4. Open **Scan**, choose a photo of an ingredients list and tap **Scan ingredients**.
5. Review the warning and tap **Save to history**, then check the **History** tab.

## Notes

- The `NoAllergen` / `AllergenDetected` values match the `ScanFlag` enum in the
  backend, and are sent both as JSON and as multipart form fields.
- The image is uploaded as `multipart/form-data` together with `text` and `flag`.
- `AllergenDetectionService` normalizes accents and matches whole words, so
  "egg" does not match "eggplant" and "soy" matches "soy lecithin".
