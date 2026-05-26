# LinkMeIn

LinkMeIn is a personal LinkedIn post planning, drafting, and publishing assistant. It supports local-first drafting plus an optional backend API mode for LinkedIn connection and user-triggered publishing.

## Current Capabilities

- Dashboard metrics for local drafts.
- Post list with create, edit, preview, delete, and sandbox publish actions.
- Post composer with Reactive Forms validation, AI Assist, and one optional image per post.
- LinkedIn-style sandbox preview without official LinkedIn branding.
- Month calendar and upcoming scheduled posts list.
- Local draft storage in browser `localStorage`.
- Optional backend API mode for saved posts and media.
- Backend-only LinkedIn OAuth.
- User-triggered LinkedIn publishing for text-only posts and posts with one image.
- Appearance settings for light, dark, and purple accent modes.

## Sandbox And Real Publishing

Sandbox publish updates local draft status to `mock-published` and stores a local timestamp. It is not posted to LinkedIn.

Real LinkedIn publishing is available from the preview page when using backend API mode with an active LinkedIn connection. Angular never receives LinkedIn client secrets or access tokens.

LocalStorage mode stores drafts and images only in this browser. Clearing browser storage may remove drafts.

## Not Implemented Yet

- Multiple-image publishing.
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
