import { Observable } from 'rxjs';

import { PostDraft } from '../models/post-draft.model';

export abstract class DraftStoreService {
  abstract listDrafts(): Observable<PostDraft[]>;
  abstract getDraft(id: string): Observable<PostDraft | undefined>;
  abstract saveDraft(draft: PostDraft): Observable<PostDraft>;
  abstract deleteDraft(id: string): Observable<void>;
}
