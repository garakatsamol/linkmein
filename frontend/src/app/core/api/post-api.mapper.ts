import { DraftImage } from '../models/draft-image.model';
import { PostDraft, PostStatus } from '../models/post-draft.model';
import { DraftPayload } from '../services/draft-store.service';
import { ApiPostDto, ApiPostStatus, CreateApiPostRequest, UpdateApiPostRequest } from './models/api-post.model';

const emptyImages: DraftImage[] = [];

export function mapApiPostToPostDraft(post: ApiPostDto): PostDraft {
  return {
    id: post.id,
    title: post.title,
    content: post.content,
    images: emptyImages,
    scheduledFor: post.scheduledFor ?? undefined,
    status: mapApiStatusToPostStatus(post.status),
    createdAt: post.createdAt,
    updatedAt: post.updatedAt,
    mockPublishedAt: post.status === 'Published' ? post.publishedAt ?? undefined : undefined
  };
}

export function mapDraftPayloadToCreateApiPostRequest(payload: DraftPayload): CreateApiPostRequest {
  return {
    title: payload.title,
    content: payload.content,
    scheduledFor: payload.scheduledFor
  };
}

export function mapDraftPayloadToUpdateApiPostRequest(payload: DraftPayload): UpdateApiPostRequest {
  return {
    title: payload.title,
    content: payload.content,
    scheduledFor: payload.scheduledFor
  };
}

function mapApiStatusToPostStatus(status: ApiPostStatus): PostStatus {
  switch (status) {
    case 'Scheduled':
      return 'scheduled';
    case 'Published':
      return 'mock-published';
    case 'Draft':
    case 'Publishing':
    case 'Failed':
      // The Phase 1 UI only understands local draft/scheduled/mock-published states.
      return 'draft';
  }
}
