import { DraftImage } from './draft-image.model';

export interface LinkedinPreview {
  authorName: string;
  authorHeadline: string;
  content: string;
  images: DraftImage[];
  timestampLabel: string;
}
