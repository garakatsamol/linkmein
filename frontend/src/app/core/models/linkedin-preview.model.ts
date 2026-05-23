import { DraftImage } from './draft-image.model';
import { PostStatus } from './post-draft.model';

export interface LinkedinPreview {
  authorName: string;
  authorHeadline: string;
  content: string;
  images: DraftImage[];
  scheduledFor?: string;
  status: PostStatus;
  timestampLabel: string;
}
