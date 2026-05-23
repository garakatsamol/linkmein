# LinkMeIn Architecture

LinkMeIn is organized as a monorepo:

- `frontend/` contains the Angular app using PrimeNG Sakai.
- `backend/` is a placeholder for the future .NET Web API.
- `docs/` contains setup and architecture notes.

The Angular app must not call LinkedIn APIs, store LinkedIn client secrets, implement OAuth, publish to LinkedIn directly, scrape LinkedIn, read feed/contact/connection data, or automate LinkedIn behavior.
