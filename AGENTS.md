# LinkMeIn Repository Guidance

LinkMeIn is a personal LinkedIn post calendar and publishing assistant.

This repository should prioritize a clean, focused MVP before adding integrations or backend complexity.

## Project Structure

- `frontend/` - Angular app using PrimeNG Sakai.
- `backend/` - Future .NET Web API. Placeholder only for now.
- `docs/` - Architecture and setup documentation.

## Current Phase

Phase 1 is Angular-only.

Do not implement backend code yet.

## Angular MVP Scope

The MVP should include:

- Dashboard.
- Post calendar.
- Post list.
- Post composer.
- Image upload and local preview.
- LinkedIn-style sandbox preview.
- Local draft storage using `localStorage` or IndexedDB.
- Mock publishing only.
- Disabled or placeholder LinkedIn connection page.
- Disabled or placeholder real publish action explaining that real LinkedIn publishing requires a backend.

## Hard Restrictions

- Do not call LinkedIn APIs directly from Angular.
- Do not store LinkedIn client secrets in Angular.
- Do not implement LinkedIn OAuth yet.
- Do not implement real LinkedIn publishing yet.
- Do not scrape LinkedIn.
- Do not read LinkedIn feed data.
- Do not read contacts or connections.
- Do not implement auto-like, auto-comment, auto-connect, auto-DM, or browser automation.

## Frontend Rules

- Use Angular.
- Use PrimeNG.
- Use the Sakai theme/template.
- Use Reactive Forms.
- Prefer PrimeNG components.
- Keep the UI clean and MVP-focused.
- Use service abstractions so `MockPublisherService` can later be replaced by `ApiPublisherService`.

## Backend Rules

- Create only a `backend/` placeholder if needed.
- Do not implement .NET backend code until explicitly requested.
- The future backend will handle LinkedIn OAuth, token storage, media upload to LinkedIn, real publishing, scheduled publishing, and publish audit logs.

## Coding Rules

- Prefer small, commit-sized changes.
- Avoid overengineering.
- Build the MVP first.
- Do not implement the Angular app until explicitly requested.

Before implementation, present:

1. Folder structure.
2. Data models.
3. Services.
4. Routes.
5. Implementation steps.

## Implementation Boundary

For now, repository guidance may be created or updated. Do not start building the Angular app, backend, LinkedIn integration, OAuth, publishing pipeline, scraping, feed reading, contact reading, connection reading, automation, or browser-based LinkedIn workflows unless explicitly requested in a future task.
