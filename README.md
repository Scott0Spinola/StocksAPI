# StocksAPI

ASP.NET Core Web API for managing `Cliente_Tag` and `Cliente_Movimento` records, backed by Entity Framework Core (SQL Server) and protected by an API-key header.

## Tech stack

- .NET: `net10.0`
- ASP.NET Core Web API + Controllers
- EF Core 10 (SQL Server)
- Swagger (Development only)
- Serilog file + console logging

## Project layout

- `src/` — Web API project
- `src.Tests/` — Test project

## Quick start

Prerequisites:

- .NET SDK that supports `net10.0`
- SQL Server (or SQL Express) reachable from your machine

From the repo root:

```powershell
dotnet restore .\src.slnx
dotnet build .\src.slnx

# run (Development profile uses HTTPS + Swagger)
dotnet run --project .\src\src.csproj
```

Local URLs (Development profile):

- HTTP: `http://localhost:5275`
- HTTPS: `https://localhost:7092`

Swagger UI (Development only):

- `https://localhost:7092/swagger`
- `http://localhost:5275/swagger`

## Configuration

### Database

The API uses the connection string `ConnectionStrings:DefaultConnection`.

- Environment variable equivalent: `ConnectionStrings__DefaultConnection`

- `src/appsettings.json` contains a default SQL Server/SQL Express connection string.
- `src/appsettings.Development.json` adds `Encrypt=True;TrustServerCertificate=True;` and disables auto-migrations by default.

Optional migration auto-apply:

- `Database:ApplyMigrations` (boolean)
  - Environment variable equivalent: `Database__ApplyMigrations`
  - When `true`, the API runs `db.Database.Migrate()` on startup.
  - Default in Development is `false` to avoid accidentally creating/updating tables against an existing database.

Example (PowerShell) overriding the connection string and enabling migrations:

```powershell
$env:ConnectionStrings__DefaultConnection = "Server=localhost\\SQLEXPRESS;Database=StocksApi;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=True"
$env:Database__ApplyMigrations = "true"
```

### Authentication (API Key)

All controller routes require authentication.

- Header: `X-API-KEY`
- Config key: `Authentication:ApiKey`
- Environment variable equivalent: `Authentication__ApiKey`

Example (PowerShell):

```powershell
$headers = @{ "X-API-KEY" = "YOUR_KEY" }
Invoke-RestMethod "https://localhost:7092/api/Cliente_Tag_?pageNumber=1&pageSize=20" -Headers $headers
```

Example (PowerShell) setting the API key via environment variable:

```powershell
$env:Authentication__ApiKey = "YOUR_KEY"
```

## Running locally (VS Code tasks)

This repo includes tasks for common workflows:

- `build`: `dotnet build src.slnx`
- `watch`: `dotnet watch run --project src.slnx`
- `publish`: `dotnet publish src.slnx`

## Running with Docker Compose

A compose file is provided at `src/docker-compose.yml`.

```powershell
cd .\src

docker compose up --build
```

By default it maps container port `5275` to host `5275`:

- `http://localhost:5275`
- Swagger (Development): `http://localhost:5275/swagger`

Note: the compose file only runs the API container. You still need a reachable SQL Server instance (and a correct `DefaultConnection` via config/environment).

## Running with Docker (single container)

If you have a prebuilt image, you can run it while supplying configuration via environment variables.

Example:

```powershell
docker run --rm -p 5275:5275 `
  -e Authentication__ApiKey="YOUR_KEY" `
  -e ConnectionStrings__DefaultConnection="<YOUR_SQLSERVER_CONNECTION_STRING>" `
  kbairesearch/development:movimentoapi.3.0
```

## API endpoints

Base path is `api/[controller]`. With the current controller class names, the routes are:

### Tags (`Cliente_Tag_Controller`)

Base: `/api/Cliente_Tag_`

- `GET /api/Cliente_Tag_?pageNumber=1&pageSize=20`
- `GET /api/Cliente_Tag_/{id:int}`
- `GET /api/Cliente_Tag_/Unidade?unidade=...`
- `GET /api/Cliente_Tag_/Localizacao?localizacao=...`
- `GET /api/Cliente_Tag_/Produto?produto=...`
- `GET /api/Cliente_Tag_/EPC?epc=...`
- `GET /api/Cliente_Tag_/Estado?estado=...`
- `POST /api/Cliente_Tag_`
- `PUT /api/Cliente_Tag_?id={id:int}`
- `DELETE /api/Cliente_Tag_/{id:int}`

### Movimentos (`Cliente_movimento_Controller`)

Base: `/api/Cliente_movimento_`

- `GET /api/Cliente_movimento_?pageNumber=1&pageSize=20`
- `GET /api/Cliente_movimento_/Por-Data?start=2026-01-01&end=2026-01-31&pageNumber=1&pageSize=20`
- `GET /api/Cliente_movimento_/{id:int}`
- `GET /api/Cliente_movimento_/RID?rid=...`
- `GET /api/Cliente_movimento_/Cliente?cliente=...`
- `POST /api/Cliente_movimento_`
- `PUT /api/Cliente_movimento_/{id:int}`
- `DELETE /api/Cliente_movimento_/{id:int}`

## Logging & errors

- Logs are written to `src/Logs/` with daily rolling files (`log-YYYYMMDD.txt`) and a retention of 7 files.
- Exceptions are handled via `IExceptionHandler` and returned as RFC 7807 Problem Details, including `traceId`.

## Tests

```powershell
dotnet test .\src.Tests\src.Tests.csproj
```
