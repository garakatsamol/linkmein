# LinkMeIn

LinkMeIn is a personal LinkedIn post calendar and publishing assistant. Phase 1 is an Angular-only MVP focused on local drafting, planning, previewing, and sandbox publishing.

## Phase 1 Capabilities

- Dashboard metrics for local drafts.
- Post list with create, edit, preview, delete, and sandbox publish actions.
- Post composer with Reactive Forms validation.
- Multiple local image previews per draft.
- LinkedIn-style sandbox preview without official LinkedIn branding.
- Month calendar and upcoming scheduled posts list.
- Local draft storage in browser `localStorage`.
- LinkedIn settings placeholder explaining the future backend boundary.

## Sandbox Only

Sandbox publish updates local draft status to `mock-published` and stores a local timestamp. It is not posted to LinkedIn.

Drafts and images are stored only in this browser. Clearing browser storage may remove drafts. Cloud storage and database persistence are future backend work.

## Not Implemented Yet

- LinkedIn OAuth.
- Real LinkedIn publishing.
- LinkedIn API calls.
- Backend API.
- Database/cloud storage.
- Media upload to LinkedIn.
- Scheduled production publishing.
- Feed, contact, or connection reading.
- Auto-like, auto-comment, auto-connect, auto-DM, scraping, or browser automation.

## Run Locally

```powershell
cd frontend
npm install
npm start
```

Open `http://127.0.0.1:4200`.

## Frontend Storage Mode

The Angular app defaults to browser `localStorage` mode in `frontend/src/environments/environment.ts`.
To test the local backend API during development, run the .NET API and set a browser override in DevTools:

```js
localStorage.setItem('linkmein:storageMode', 'api');
location.reload();
```

Return to local mode with:

```js
localStorage.setItem('linkmein:storageMode', 'local');
location.reload();
```

Or remove the override and fall back to `environment.storageMode`:

```js
localStorage.removeItem('linkmein:storageMode');
location.reload();
```

Do not commit API mode as the default.

## Build And Test

```powershell
cd frontend
npm run build
npm test -- --watch=false
```

## Known Warning

`npm run build` currently reports Angular's initial bundle budget warning because the PrimeNG theme and utility styles push the initial bundle over the default 500 kB warning budget. The build still completes successfully.
