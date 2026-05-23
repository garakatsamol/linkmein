import { Observable } from 'rxjs';

import { PostDraft } from '../models/post-draft.model';
import { PublishResult } from '../models/publish-result.model';

export abstract class PublisherService {
  abstract publishDraft(draft: PostDraft): Observable<PublishResult>;
}
