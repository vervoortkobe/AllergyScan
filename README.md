# AllergyScan

.NET MAUI app with a WebAPI backend for the AI.NET course. AllergyScan helps people with food allergies check the ingredients of a product by photographing the label, using Azure AI Computer Vision for OCR, Azure Translator for language conversion, and a local allergen detection service to flag allergens.

## Requirements & Implementation

| Requirement | Implementation |
|-------------|----------------|
| **.NET MAUI + MVVM Community Toolkit** | All pages use ViewModels inheriting from `ObservableObject`/`ObservableRecipient` with `[RelayCommand]` from CommunityToolkit.Mvvm. Code-behind files are minimal/empty (only InitializeComponent and QueryAttributes). |
| **Azure AI Model Integration** | `IOCRService` uses Azure Computer Vision for OCR. `ITranslationService` uses Azure AI Translator. Multiple AI services are used as required. API keys configured in MAUI appsettings. |
| **Attractive, user-friendly interface** | White + dark green theme, shadows on cards/borders, custom converters for flag colors, responsive layouts, setup flow, clear "eatable/non-eatable" result screens. |
| **WebAPI in .NET, read AND write data** | Full CRUD on Allergies, Scans, and Language. All endpoints accept/return data. POST/PUT/DELETE operations implemented. |
| **At least 2 tables connected by one-to-many relationship** | **Language** (1) → **UserSettings** (N), **ScanHistory** (1) → **ScanAllergy** (N), **Allergy** (1) → **ScanAllergy** (N). Scan ↔ Allergy is a many-to-many through `ScanAllergy`. All tables are used by the app (language selection, allergy profile, scan history with detected allergens). |
| **Layered Architecture, Repository Design Pattern, async/await** | **Layers**: `AllergyScan.WebAPI` (Controllers/Startup), `AllergyScan.BLL` (Services/DTOs/Interfaces), `AllergyScan.DAL` (Repositories/Contexts/Configurations/Models). **Repositories**: `IRepository<T>`, `AllergyRepository`, `ScanRepository`, `LanguageRepository`. **Unit of Work**: `IUnitOfWork` with `CompleteAsync()`. All DB/API calls async. |
| **Host API and DB locally with Ngrok** | API runs locally via `dotnet run` or Docker Compose (`docker-compose.yml`). SQLite DB stored locally. Expose via `ngrok http <port>` as documented. |
| **Multiple pages, Navigation Service, at least 2 messages** | 8+ pages (Scan, Result, ScanDetails, History, Allergies, Settings, SetupLanguage, SetupAllergies). `INavigationService` implemented. Messages: `RefreshScansMessage`, `LanguageChangedMessage` via WeakReferenceMessenger. |
| **Creative (not just SafariSnap clone)** | Allergy-specific detection with 4 severity levels, Dutch "kan bevatten/kan sporen bevatten" parsing with sentence-aware lookback, multi-language may-contain phrases, allergy profile management, severity cycling, custom flag UI per state. |
| **Naming conventions and coding guidelines** | PascalCase for classes/methods/properties, interfaces prefixed with I, DTOs suffixed appropriately, proper folder structure (ViewModels/Views/Services/Models), async methods suffixed with Async. |

## Database Schema (Multiple One-to-Many Relationships)

```
Language (1) ──< UserSettings (N)
    │              │
    Id             Id, LanguageId (FK), ...
    Code           singleton (Id=1) in single-user app
    Name
    UserSettings (collection)

ScanHistory (1) ──< ScanAllergy (N) >── (1) Allergy
    │                    │                      │
    Id                   ScanId (FK)            Id
    Text                 AllergyId (FK)         Name
    Flag                 DetectionFlag          Flag
    ScannedAt            Notes                  CreatedAt
    UserId               UserId
    ScanAllergies        Scan / Allergy navs    ScanAllergies
```

### Relationships

- **Language (1) → UserSettings (N)**: a language can be referenced by user settings; `UserSettings.LanguageId` FK with restrict delete.
- **ScanHistory (1) → ScanAllergy (N)**: one scan can contain **multiple allergens**.
- **Allergy (1) → ScanAllergy (N)**: one allergy can appear in many scans.
- **ScanHistory ↔ Allergy**: many-to-many through the `ScanAllergies` junction table (composite PK `ScanId + AllergyId`).

### Implicit user ownership (single-user app)

There is intentionally **no separate `Users` table** yet. Ownership is implicit and modelled with a `UserId` column (default `1`) on `Allergies`, `Scans` and `ScanAllergies`. This keeps the current single-user behaviour while making a future multi-user migration a straightforward filter/backfill instead of a schema redesign. Indexes on `UserId` and on `(UserId, ...)` composites make those future queries efficient.

### Shared `ScanFlag` enum

`ScanFlag` lives in one place: `AllergyScan.DAL.Model.ScanFlag`. It is intentionally reused with **different semantics**:

- `Allergy.Flag` — the user's severity preference for that allergy (e.g. "I react strongly to peanuts").
- `ScanHistory.Flag` — the overall severity detected for this scan.
- `ScanAllergy.DetectionFlag` — the severity detected for that specific allergen in that scan.

Keeping one enum avoids duplicated values while the XML documentation explains the semantic difference.

## Architecture

### Mobile App (.NET MAUI + MVVM Community Toolkit)

```
camera/gallery -> photo
        |
Azure Computer Vision (OCR) -> text
        |
Azure Translator -> translated text
        |
Local allergen detection -> flag (NoAllergen | Contains | MayContain | MayContainTraces)
        |
        +--- POST /api/scans ---------------> scans + image
        +--- GET/PUT /api/allergies --------> allergy profile
        +--- GET/PUT /api/languages --------> selected language
```

### WebAPI (.NET 8 / ASP.NET Core)

Layered structure with Repository + Unit of Work pattern.

## Key Features

- **OCR**: Images >3.8MB downscaled to max 1600px and re-encoded as JPEG @85% before Azure OCR.
- **Azure AI Integration**: Computer Vision (OCR) + Translator with proper error handling.
- **Allergen Detection**: 4-state severity with correct precedence - "may contain" never downgraded to "contains". Context-aware detection with sentence boundaries.
- **Multi-language Support**: EN/NL/FR/DE/ES with translations and allergen name translations.
- **Navigation**: Fixed post-scan flow - goes to Result page, back button returns to Scan tab without loops.

## Stack

- **Mobile**: .NET MAUI, MVVM Community Toolkit, CommunityToolkit.Mvvm.Messaging, SkiaSharp
- **WebAPI**: ASP.NET Core 10, EF Core 10 + SQLite, Repository Pattern, Unit of Work, Swagger/OpenAPI
- **AI**: Azure Computer Vision, Azure AI Translator
- **Database**: SQLite (local), Docker Compose

## Project Structure

```
AllergyScan/
├─ webapi/AllergyScan/
│  ├─ AllergyScan.BLL/      # Services, DTOs, Interfaces, Exceptions
│  ├─ AllergyScan.DAL/      # Repositories, Contexts, Configurations, Models, Migrations
│  └─ AllergyScan.WebAPI/   # Controllers, Startup, Middleware
├─ mobile/AllergyScan/      # MAUI app (ViewModels, Views, Services, Models)
├─ docker-compose.yml
└─ README.md
```

## API Documentation (Swagger)

When running the WebAPI locally, **Swagger UI** is available at:
- **Local dev**: `http://localhost:5236/swagger`
- **Docker**: `http://localhost:8080/swagger`

Swagger provides interactive API documentation for all endpoints (Allergies, Languages, Scans) with request/response schemas and the ability to test endpoints directly.

## API Endpoints

### Allergies
| Method | Route | Description |
|---|---|---|
| `GET` | `/api/allergies` | List all allergies |
| `GET` | `/api/allergies/{id}` | Get allergy by id |
| `POST` | `/api/allergies` | Add allergy |
| `PUT` | `/api/allergies/{id}` | Update allergy |
| `DELETE` | `/api/allergies/{id}` | Delete allergy |

### Languages
| Method | Route | Description |
|---|---|---|
| `GET` | `/api/languages` | List supported languages |
| `GET` | `/api/languages/current` | Get current language |
| `PUT` | `/api/languages/current` | Set current language |

### Scans
| Method | Route | Description |
|---|---|---|
| `POST` | `/api/scans` | Store processed scan (multipart/form-data) |
| `GET` | `/api/scans?page=1&pageSize=20` | Paged history |
| `GET` | `/api/scans/{id}` | Get scan by id |
| `GET` | `/api/scans/{id}/image` | Get scan image |
| `PUT` | `/api/scans/{id}` | Update scan text/flag/allergy ids |
| `PUT` | `/api/scans/{id}/allergies` | Replace the allergies detected in a scan |
| `PUT` | `/api/scans/{id}/image` | Replace image |
| `DELETE` | `/api/scans/{id}` | Delete scan |
| `DELETE` | `/api/scans` | Clear history |

## Quick Start

### 1. Start the backend (Docker)
```bash
docker compose up --build
```
API runs on `http://localhost:8080`. Swagger at `http://localhost:8080/swagger`.

### 2. Run locally without Docker
```bash
cd webapi/AllergyScan
dotnet run --project AllergyScan.WebAPI
```
API typically runs on `http://localhost:5236`. Swagger at `http://localhost:5236/swagger`.

### 3. Expose with Ngrok (optional)
```bash
ngrok http 5236
```
Use the provided ngrok URL in your mobile app configuration.

### 4. Run the MAUI app
```bash
cd mobile/AllergyScan
dotnet build -t:Run -f net10.0-windows10.0.19041.0
```
- Windows: connects to `http://localhost:8080`
- Android emulator: connects to `http://10.0.2.2:8080`

## Pages & Navigation

- **ScanPage** — Photo capture/pick → OCR → detection → Result
- **ResultPage** — Shows eatable/non-eatable status, allergen details
- **ScanDetailsPage** — View/edit scan, change flag
- **HistoryPage** — Past scans, refreshes via messages
- **AllergiesPage** — Manage allergy profile
- **SettingsPage** — Language selection
- **SetupLanguagePage** → **SetupAllergiesPage** — First-time setup

## Messages
- `RefreshScansMessage` — Refresh history after changes
- `LanguageChangedMessage` — Update UI on language change

## Architecture Compliance
- **Layered Architecture** ✓ (WebAPI/BLL/DAL)
- **Repository Pattern** ✓ (`IRepository<T>`, concrete repos)
- **Unit of Work** ✓ (`IUnitOfWork.CompleteAsync()`)
- **Async/Await** ✓ throughout
- **MVVM Community Toolkit** ✓
- **Read/Write via API** ✓
- **≥2 tables, 1:N** ✓ (Language → UserSettings, ScanHistory → ScanAllergy, Allergy → ScanAllergy, plus Scan ↔ Allergy many-to-many)
- **NavigationService + ≥2 Messages** ✓
- **Azure AI integrated** ✓
- **Creative, not SafariSnap clone** ✓