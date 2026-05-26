# LinkMeIn Setup

## Frontend

```powershell
cd frontend
npm install
npm start
```

## Backend

```powershell
cd backend
dotnet build LinkMeIn.Backend.sln
dotnet run --project LinkMeIn.Api
```

Use backend API mode in the frontend only when you want to test server-backed posts, media, LinkedIn OAuth, AI Assist, or user-triggered LinkedIn publishing.

## Manual Testing

Use [manual-test-checklist.md](manual-test-checklist.md) for the current manual QA pass.
