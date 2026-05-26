# Manual Test Checklist

Use this checklist for the current LinkMeIn MVP.

## Drafts

- Create a draft from `/composer`.
- Confirm title and content validation messages appear when required fields are empty.
- Edit an existing draft from `/posts`.
- Delete a draft from `/posts`.
- Confirm drafts persist after refreshing the browser.

## Images

- Add one valid image to a draft.
- Confirm adding another image is blocked with "Only one image is supported for now."
- Confirm unsupported image types show validation.
- Confirm images over 4 MB show validation.
- Remove an image from a draft.
- Save and reopen the draft to confirm images remain.

## Preview

- Open a draft preview from `/posts`.
- Confirm post content line breaks are preserved.
- Confirm status, scheduled date, and images appear.
- Confirm the preview says it is not posted to LinkedIn.

## Sandbox Publishing

- Sandbox publish a draft from `/posts`.
- Sandbox publish a draft from `/posts/:id/preview`.
- Confirm status changes to `mock-published`.
- Confirm a local published timestamp appears.
- Confirm the UI says the post was not posted to LinkedIn.

## Calendar

- Open `/calendar`.
- Move to previous and next months.
- Filter by All, Draft, Scheduled, and Mock-published.
- Click a calendar post and confirm it opens preview.
- Click a day action and confirm Composer opens with the scheduled date prefilled.
- Confirm upcoming scheduled posts appear from today onward.

## Dashboard

- Open `/dashboard`.
- Confirm metrics reflect local drafts.
- Confirm quick actions navigate to Composer, Posts, Calendar, and LinkedIn settings.
- Confirm recent/upcoming post actions open Preview and Edit.

## LinkedIn Settings

- Open `/linkedin`.
- Confirm the page states OAuth and token storage are backend-only.
- Confirm Angular does not expose LinkedIn secrets or tokens.
- Confirm `Connect LinkedIn` starts the backend OAuth flow when disconnected.
- Confirm the publishing status says text and one image are supported.
- Confirm multiple images and scheduled publishing are shown as not implemented.
