# StocksAPI (src)

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

## Running locally

Prerequisites:

- .NET SDK that supports `net10.0`
- SQL Server (or SQL Express) accessible from your machine

From the repo root:

```powershell
cd .\src

dotnet restore

dotnet run
```

Local URLs (Development profile):

- HTTP: `http://localhost:5275`
- HTTPS: `https://localhost:7092`

Swagger UI (Development only):

- `https://localhost:7092/swagger`
- `http://localhost:5275/swagger`

## Running with Docker Compose

A compose file is provided at `src/docker-compose.yml`.

```powershell
cd .\src

docker compose up --build
```

By default it maps container port `8080` to host `8080`:

- `http://localhost:8080`

Note: the compose file only runs the API container. You still need a reachable SQL Server instance (and a correct `DefaultConnection` via config/environment).

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
