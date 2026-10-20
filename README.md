# AllergyScan

.NET MAUI app with a WebAPI backend for the AI.NET course.

- `webapi/AllergyScan` — ASP.NET Core 10 backend with SQLite, allergies, language selection and scan history. OCR, translation and allergen detection run in the MAUI app. See [the backend README](webapi/AllergyScan/README.md).
- `mobile` — .NET MAUI client (to be built).
- `docker-compose.yml` — runs the backend on <http://localhost:8080>.

Quick start:

```bash
docker compose up --build
```

Swagger: <http://localhost:8080/swagger>
