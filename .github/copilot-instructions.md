# Copilot Instructions for LinkMeIn

LinkMeIn is a personal LinkedIn post calendar and publishing assistant.

## Project State
- Angular Phase 1 MVP is complete.
- Phase 2 backend foundation exists.
- Continue with small backend slices only.

## Critical Rules
- **Never call LinkedIn APIs directly from Angular.**
- **Never store LinkedIn secrets in Angular.**
- LinkedIn OAuth, token storage, real publishing, and media upload to LinkedIn must be backend-only.
- Do not implement scraping, feed reading, contact reading, connection reading, auto-like, auto-comment, auto-connect, auto-DM, or browser automation.

## Implementation Guidance
- Do not implement any app feature unless explicitly requested.
- All LinkedIn integration must be backend-only.
- Angular frontend must use service abstractions and never handle LinkedIn credentials or direct API calls.
- Prefer small, focused backend changes.

## Project Structure
- `frontend/` – Angular app (PrimeNG, Sakai theme)
- `backend/` – .NET Web API foundation (backend-only for LinkedIn integration)
- `docs/` – Documentation

---

**Follow these instructions for all Copilot suggestions and completions in this repository.**
