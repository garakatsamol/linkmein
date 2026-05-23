# LinkMeIn Backend

Phase 2 starts the minimal .NET backend foundation for future real LinkedIn publishing.

This slice includes:

- `LinkMeIn.Api` .NET Web API.
- `LinkMeIn.Api.Tests` xUnit test project.
- SQL Server EF Core model and initial migration.
- `GET /api/health`.
- Local Angular CORS for `http://localhost:4200` and `http://127.0.0.1:4200`.
- Configuration placeholders for SQL Server, LinkedIn OAuth, and token encryption.

This slice does not implement LinkedIn OAuth, token exchange, media upload, real publishing, scheduled publishing, or frontend integration.

## Local Setup

```powershell
cd backend
dotnet restore
dotnet build
dotnet test
```

## Run API

```powershell
cd backend
dotnet run --project LinkMeIn.Api
```

Health check:

```text
GET /api/health
```

## Database

The default development connection string uses SQL Server LocalDB:

```text
Server=(localdb)\mssqllocaldb;Database=LinkMeIn_Development;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True
```

Apply migrations:

```powershell
cd backend
dotnet tool restore
dotnet ef database update --project LinkMeIn.Api
```

## Secrets

Do not store production secrets in `appsettings.json`.

Configure these via user secrets, environment variables, or a deployment secret store:

- `ConnectionStrings__DefaultConnection`
- `LinkedIn__ClientId`
- `LinkedIn__ClientSecret`
- `LinkedIn__RedirectUri`
- `TokenEncryption__Key`

LinkedIn tokens must remain server-side only. Angular must never store LinkedIn client secrets or access tokens.
