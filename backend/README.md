# LinkMeIn Backend

Phase 2 starts the minimal .NET backend foundation for future real LinkedIn publishing.

This slice includes:

- `LinkMeIn.Api` .NET Web API.
- `LinkMeIn.Api.Tests` xUnit test project.
- SQL Server EF Core model and initial migration.
- `GET /api/health`.
- Backend-only LinkedIn OAuth connection foundation.
- Local Angular CORS for `http://localhost:4200` and `http://127.0.0.1:4200`.
- Configuration placeholders for SQL Server, LinkedIn OAuth, and token encryption.

This backend does not implement LinkedIn post publishing, media upload to LinkedIn, scheduled publishing, or frontend LinkedIn integration yet.

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

LinkedIn OAuth foundation:

```text
GET  /api/linkedin/status
GET  /api/linkedin/oauth/start
GET  /api/linkedin/oauth/callback
POST /api/linkedin/disconnect
```

Tokens are exchanged and stored server-side only. The callback response never returns access or refresh tokens to the browser.

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
- `LinkedIn__AuthorizationEndpoint`
- `LinkedIn__TokenEndpoint`
- `LinkedIn__Scopes__0`
- `LinkedIn__Scopes__1`
- `LinkedIn__Scopes__2`
- `LinkedIn__Scopes__3`
- `TokenEncryption__Key`

For local OAuth testing, configure the LinkedIn Developer Portal redirect URL to exactly match `LinkedIn__RedirectUri`, for example:

```text
https://localhost:7161/api/linkedin/oauth/callback
```

The current local token protection uses ASP.NET Core Data Protection. Production deployments should use managed key storage and persistent key rings. LinkedIn tokens must remain server-side only. Angular must never store LinkedIn client secrets or access tokens.
