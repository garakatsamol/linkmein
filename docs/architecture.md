# LinkMeIn Architecture

LinkMeIn is organized as a monorepo:

- `frontend/` contains the Angular app using PrimeNG Sakai.
- `backend/` contains the .NET Web API for posts, media, LinkedIn OAuth, user-triggered publishing, and AI Assist.
- `docs/` contains setup and architecture notes.

The Angular app must not call LinkedIn APIs, store LinkedIn client secrets, publish to LinkedIn directly, scrape LinkedIn, read feed/contact/connection data, or automate LinkedIn behavior. LinkedIn OAuth, tokens, media upload to LinkedIn, and publishing stay backend-only.
