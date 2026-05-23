import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';

import { PostDraft } from '../models/post-draft.model';
import { PublishResult } from '../models/publish-result.model';
import { DraftStoreService } from './draft-store.service';
import { PublisherService } from './publisher.service';

@Injectable({ providedIn: 'root' })
export class MockPublisherService extends PublisherService {
  private readonly draftStore = inject(DraftStoreService);

  override publishDraft(draft: PostDraft): Observable<PublishResult> {
    const publishedAt = new Date().toISOString();

    return this.draftStore
      .updateDraft(draft.id, {
        title: draft.title,
        content: draft.content,
        images: draft.images,
        mockPublishedAt: publishedAt,
        scheduledFor: draft.scheduledFor,
        status: 'mock-published'
      })
      .pipe(
        map(() => ({
          success: true,
          mode: 'mock',
          message: 'Sandbox publish complete. This was not posted to LinkedIn.',
          publishedAt
        }))
      );
  }
}
