import { Observable } from 'rxjs';

import { DraftImage } from '../models/draft-image.model';
import { PostDraft, PostStatus } from '../models/post-draft.model';

export interface DraftPayload {
  title: string;
  content: string;
  images?: DraftImage[];
  scheduledFor?: string;
  status?: PostStatus;
}

export abstract class DraftStoreService {
  abstract listDrafts(): Observable<PostDraft[]>;
  abstract getDraft(id: string): Observable<PostDraft | undefined>;
  abstract createDraft(payload: DraftPayload): Observable<PostDraft>;
  abstract updateDraft(id: string, payload: DraftPayload): Observable<PostDraft>;
  abstract deleteDraft(id: string): Observable<void>;
}
