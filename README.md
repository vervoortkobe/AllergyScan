# AllergyScan

.NET MAUI app with a WebAPI backend for the AI.NET course.

AllergyScan helps people with food allergies check the ingredients of a product.
The user photographs the ingredients list, Azure AI Computer Vision reads the text
(OCR), Azure AI Translator translates it to the user's language, and the app compares
the result with the user's personal allergy profile and shows a clear warning.

- `webapi/AllergyScan` — ASP.NET Core 10 backend with SQLite, allergies, language selection and scan history. See [the backend README](webapi/AllergyScan/README.md).
- `mobile/AllergyScan` — .NET MAUI client (MVVM) that runs the OCR, translation and allergen check. See [the app README](mobile/AllergyScan/README.md).
- `docker-compose.yml` — runs the backend on <http://localhost:8080>.

## Architecture

```
MAUI app                                     WebAPI (SQLite)
--------                                     ---------------
camera/gallery -> photo
        |
Azure Computer Vision (OCR) -> text
        |
Azure Translator -> translated text
        |
allergen check against the profile ->
        |
   flag (NoAllergen | AllergenDetected)
        |
        +--- POST /api/scans ---------------> scans + image
        +--- GET/PUT /api/allergies --------> allergy profile
        +--- GET/PUT /api/languages --------> selected language
```

The backend only stores the user's allergies, the selected language and the scan
history. All AI work happens inside the MAUI app, so the Azure keys never leave the
device.

## Quick start

1. Start the backend:

   ```bash
   docker compose up --build
   ```

2. Run the MAUI app:

   ```bash
   cd mobile/AllergyScan
   dotnet build -t:Run -f net10.0-windows10.0.19041.0
   ```

   The app talks to <http://localhost:8080> on Windows and to
   <http://10.0.2.2:8080> on the Android emulator. See
   [mobile/AllergyScan/README.md](mobile/AllergyScan/README.md) to change the URL.

Swagger: <http://localhost:8080/swagger>
