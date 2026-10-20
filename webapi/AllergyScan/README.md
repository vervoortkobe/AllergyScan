# AllergyScan WebAPI

Single-user backend for the AllergyScan MAUI app. It stores the user's allergies,
the selected language and the scan history in SQLite.

The MAUI app performs the scan pipeline with Azure (OCR, translation and allergen
matching). This backend does **not** run OCR, translate text or check allergens;
it only persists the results sent by the app.

```
MAUI app: image -> Azure OCR -> text -> Azure Translator -> translated text
                                                           |
                                                  allergen check -> flag
                                                           |
                                                  POST /api/scans
                                                           |
                                              SQLite scan history
```

## Stack

- ASP.NET Core 10 Web API (`AllergyScan.WebAPI`)
- Business layer (`AllergyScan.BLL`)
- Data access layer with EF Core 10 + SQLite (`AllergyScan.DAL`)

## Run with Docker

```bash
docker compose up --build
```

- API: <http://localhost:8080>
- Swagger UI: <http://localhost:8080/swagger>
- Health: <http://localhost:8080/health>

The SQLite database is stored in the `allergyscan-data` named volume (Docker
prefixes it with the Compose project name). Delete the volume
(`docker compose down -v`) to start with an empty database.

From the Android emulator, use `http://10.0.2.2:8080` instead of `localhost`.
From a physical device, use your machine's LAN IP address.

## Run locally

```bash
cd webapi/AllergyScan
dotnet run --project AllergyScan.WebAPI
```

The API listens on <http://localhost:5236> (or the URL shown in the console) and
creates `AllergyScan.WebAPI/Data/allergyscan.db` on first start.

## Database

SQLite is configured through:

```json
"ConnectionStrings": {
  "Default": "Data Source=Data/allergyscan.db"
}
```

Migrations are applied automatically at startup. To manage them manually:

```bash
dotnet tool restore
dotnet ef migrations add <Name> --project AllergyScan.DAL --startup-project AllergyScan.WebAPI --output-dir Migrations
dotnet ef database update --project AllergyScan.DAL --startup-project AllergyScan.WebAPI
```

The database is seeded with five languages and English as the selected language.
Allergies start empty. The scan history starts empty.

## API

All request/response bodies are JSON unless noted. Enums are serialized as strings.

### Allergies

| Method | Route | Description |
| --- | --- | --- |
| `GET` | `/api/allergies` | List all allergies |
| `GET` | `/api/allergies/{id}` | Get one allergy |
| `POST` | `/api/allergies` | Add an allergy, body `{ "name": "milk" }` |
| `PUT` | `/api/allergies/{id}` | Rename an allergy, body `{ "name": "milk" }` |
| `DELETE` | `/api/allergies/{id}` | Delete an allergy |

Names are unique (case insensitive), trimmed and limited to 100 characters.
A duplicate name returns `409 Conflict`.

### Language

| Method | Route | Description |
| --- | --- | --- |
| `GET` | `/api/languages` | List supported languages (code, name) |
| `GET` | `/api/languages/current` | Get the selected language (always one) |
| `PUT` | `/api/languages/current` | Select a language, body `{ "code": "nl" }` |

Supported codes: `en`, `nl`, `fr`, `de`, `es`. The database enforces a required
language and defaults to English, so the app can never end up without a language.
An unknown code returns `400 Bad Request`.

The app uses this language as the target language for its Azure translation.

### Scans

| Method | Route | Description |
| --- | --- | --- |
| `POST` | `/api/scans` | Store a processed scan (`multipart/form-data`) |
| `GET` | `/api/scans?page=1&pageSize=20` | Paged history, newest first, without image bytes |
| `GET` | `/api/scans/{id}` | One scan, without image bytes |
| `GET` | `/api/scans/{id}/image` | The stored image |
| `PUT` | `/api/scans/{id}` | Correct the text or update the flag |
| `PUT` | `/api/scans/{id}/image` | Replace the image (`multipart/form-data`) |
| `DELETE` | `/api/scans/{id}` | Delete one scan |
| `DELETE` | `/api/scans` | Delete the whole history |

`POST /api/scans` accepts these form fields:

| Field | Required | Description |
| --- | --- | --- |
| `image` | no | Product photo (PNG, JPEG, WEBP, BMP, TIFF, GIF; max 10 MB) |
| `text` | yes | Ingredient text produced by the app's OCR provider |
| `flag` | yes | `NoAllergen` or `AllergenDetected` |

The response:

```json
{
  "id": 1,
  "text": "Ingredients: milk, eggs, wheat flour, sugar",
  "flag": "AllergenDetected",
  "scannedAt": "2026-10-20T09:05:55.8914434Z",
  "hasImage": true,
  "imageUrl": "/api/scans/1/image"
}
```

`flag` is `NoAllergen` or `AllergenDetected`. When the scan contains an image,
`imageUrl` points to `/api/scans/{id}/image` and `hasImage` is `true`.

`PUT /api/scans/{id}` body:

```json
{ "text": "Ingredients: milk and eggs", "flag": "AllergenDetected" }
```

Both fields are optional, but at least one must be supplied. The backend stores
the values as-is; it does not re-run OCR or allergen detection.

## Examples

PowerShell:

```powershell
# Add allergies
Invoke-RestMethod -Method Post -Uri http://localhost:8080/api/allergies `
  -ContentType "application/json" -Body '{"name":"milk"}'
Invoke-RestMethod -Method Post -Uri http://localhost:8080/api/allergies `
  -ContentType "application/json" -Body '{"name":"egg"}'

# Select Dutch
Invoke-RestMethod -Method Put -Uri http://localhost:8080/api/languages/current `
  -ContentType "application/json" -Body '{"code":"nl"}'

# Store a processed text-only scan
curl.exe -X POST http://localhost:8080/api/scans `
  -F "text=Ingredients: milk, eggs, wheat flour" -F "flag=AllergenDetected"

# Store a processed scan with an image
curl.exe -X POST http://localhost:8080/api/scans `
  -F "image=@ingredients.png;type=image/png" `
  -F "text=Ingredients: milk, eggs, wheat flour" -F "flag=AllergenDetected"
```

## Configuration

`appsettings.json`:

| Section | Key | Default | Meaning |
| --- | --- | --- | --- |
| `ConnectionStrings` | `Default` | `Data Source=Data/allergyscan.db` | SQLite database |
| `Scan` | `MaxImageBytes` | `10485760` | Maximum image size |
| `Scan` | `MaxTextLength` | `100000` | Maximum text length |
| `Scan` | `DefaultPageSize` | `20` | Default history page size |
| `Scan` | `MaxPageSize` | `100` | Maximum history page size |

In Docker Compose these are overridden with environment variables such as
`ConnectionStrings__Default`.

## Errors

Errors use RFC 7807 `application/problem+json`:

| Status | Situation |
| --- | --- |
| `400` | Invalid input / unsupported language / bad image type |
| `404` | Allergy or scan not found (or scan has no image) |
| `409` | Duplicate allergy |
| `500` | Unexpected server error |

## Project structure

```
AllergyScan.DAL
  Contexts/        EF Core DbContext
  Model/           Allergy, Language, UserSettings, ScanHistory, ScanFlag
  Configurations/  EF Core mappings and seed data
  Repositories/    Data access
  UOW/             Unit of work
  Migrations/      EF Core migrations

AllergyScan.BLL
  DTOs/            API contracts
  Exceptions/      Domain exceptions
  Interfaces/      Service contracts
  Options/         Configuration options
  Services/        Allergy, language and scan services

AllergyScan.WebAPI
  Controllers/     REST endpoints
  Middleware/      ProblemDetails exception handling
  Models/          Multipart form models
  Startup.cs       DI, Swagger, CORS, migrations
```
