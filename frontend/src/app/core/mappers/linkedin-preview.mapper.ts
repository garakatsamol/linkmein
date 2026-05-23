import { LinkedinPreview } from '../models/linkedin-preview.model';
import { PostDraft } from '../models/post-draft.model';

export function mapDraftToLinkedinPreview(draft: PostDraft): LinkedinPreview {
  return {
    authorName: 'Tasos Ioannidis',
    authorHeadline: 'CTO | Software Development | AI Products',
    content: draft.content,
    images: draft.images,
    scheduledFor: draft.scheduledFor,
    status: draft.status,
    timestampLabel: draft.scheduledFor ? 'Scheduled' : 'Draft preview'
  };
}
