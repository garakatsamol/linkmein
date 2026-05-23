import { DraftImage } from './draft-image.model';

export type PostStatus = 'draft' | 'scheduled' | 'mock-published';

export interface PostDraft {
  id: string;
  title: string;
  content: string;
  images: DraftImage[];
  scheduledFor?: string;
  status: PostStatus;
  createdAt: string;
  updatedAt: string;
  mockPublishedAt?: string;
}
