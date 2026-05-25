# Copilot Instructions for LinkMeIn

LinkMeIn is a personal LinkedIn post planning, drafting, and publishing assistant.

The app supports:
- Drafting LinkedIn posts
- LocalStorage mode for safe local MVP usage
- Backend API mode for real saved posts/media
- Server-side LinkedIn OAuth
- Server-side LinkedIn publishing
- AI-assisted post drafting through the backend

## Current Project State

### Frontend

- Angular app using PrimeNG/Sakai styling.
- Composer supports:
  - title/content editing
  - one optional image per post
  - AI Assist for turning rough notes into a LinkedIn draft
- Preview supports:
  - sandbox/mock publishing
  - real backend publishing
- LinkedIn Settings supports:
  - backend LinkedIn connection status
  - connect/disconnect
  - developer storage mode switch
- LocalStorage remains the default storage mode.
- API mode is enabled only through local/development override.

### Backend

- .NET Web API.
- Posts CRUD API exists.
- Media API exists:
  - upload
  - list
  - delete
  - content serving
- LinkedIn OAuth exists and is backend-only.
- LinkedIn tokens are stored server-side only.
- Real LinkedIn publishing exists:
  - text-only posts
  - one-image posts
- Multiple images are intentionally not supported.
- AI Assist backend exists:
  - Mock provider by default
  - optional Ollama provider for local free generation

## Critical Rules

- Never call LinkedIn APIs directly from Angular.
- Never store LinkedIn secrets in Angular.
- Never expose LinkedIn access tokens, refresh tokens, or client secrets to the frontend.
- LinkedIn OAuth, token storage, publishing, and media upload to LinkedIn must remain backend-only.
- Do not implement scraping, feed reading, contact reading, connection reading, auto-like, auto-comment, auto-connect, auto-DM, or browser automation.
- Do not implement scheduler unless explicitly requested.
- Do not implement autonomous posting.
- Publishing must always be user-triggered.

## AI Assist Rules

- Angular must call the backend AI endpoint only.
- Do not call AI providers directly from Angular.
- Do not add API keys to Angular.
- Default backend AI provider must remain Mock.
- Ollama may be used as an optional local provider.
- Do not add paid AI providers unless explicitly requested.
- AI Assist should help transform rough notes into a LinkedIn draft.
- AI Assist must not auto-save or auto-publish.

## Media Rules

- LinkMeIn supports one optional image per post for now.
- Frontend Composer must prevent adding more than one image.
- Backend media upload must enforce one image per post.
- LinkedIn publishing supports:
  - text-only posts
  - posts with exactly one image
- Multiple-image publishing is intentionally not supported.

## Implementation Guidance

- Do not implement any app feature unless explicitly requested.
- Prefer small, focused slices.
- With Copilot GPT-4.1, avoid large multi-feature prompts.
- Inspect before editing when the task is unclear.
- Do not do broad refactors unless explicitly requested.
- Keep existing working behavior intact.
- Prefer fixing the smallest root cause.
- If a task touches security, OAuth, token storage, or publishing, keep the scope especially small.
- Do not change storage mode default from local.
- Do not remove LocalStorage mode.
- Do not remove sandbox/mock publishing.
- Do not change backend APIs unless the task explicitly requires it.

## Validation Guidance

When changing frontend:
- Run `npm run build`
- Run `npm test -- --watch=false`

When changing backend:
- Run `dotnet build`
- Run `dotnet test`

If build/test fails because the running backend locks `LinkMeIn.Api.exe`, mention the file lock clearly instead of changing code to work around it.

## Local Development Notes

- Frontend usually runs through Angular dev server.
- Backend usually runs with the HTTPS launch profile.
- Local backend HTTPS URL is commonly:
  `https://localhost:7161`
- HTTP URL is commonly:
  `http://localhost:5062`
- Angular API base URL should use the configured environment value.
- Do not hardcode new URLs unless explicitly required for local fallback.

## Project Structure

- `frontend/` – Angular app with PrimeNG/Sakai theme
- `backend/` – .NET Web API
- `docs/` – documentation and manual test checklists

## Commit / Git Guidance

- Do not push unless explicitly asked.
- Do not commit generated local media files.
- `backend/LinkMeIn.Api/App_Data/` is local runtime media storage and must remain ignored.
- Keep commits logically grouped when possible.

## Working Style for Copilot GPT-4.1

Prefer small prompts and small changes.

Good task examples:
- Fix AI Assist loading state.
- Update Ollama prompt only.
- Add one backend test.
- Improve Composer validation message.
- Inspect publish status mapping.

Avoid broad tasks like:
- Finish AI features.
- Complete LinkedIn integration.
- Refactor frontend.
- Implement scheduler and publishing.

---

Follow these instructions for all Copilot suggestions and completions in this repository.
