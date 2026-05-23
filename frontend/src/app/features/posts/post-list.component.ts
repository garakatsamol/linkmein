import { AsyncPipe, DatePipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { MessageModule } from 'primeng/message';
import { TagModule } from 'primeng/tag';
import { Observable, Subject, startWith, switchMap } from 'rxjs';

import { PostDraft, PostStatus } from '../../core/models/post-draft.model';
import { DraftStoreService } from '../../core/services/draft-store.service';
import { PublisherService } from '../../core/services/publisher.service';

@Component({
  selector: 'app-post-list',
  imports: [AsyncPipe, ButtonModule, CardModule, DatePipe, MessageModule, RouterLink, TagModule],
  templateUrl: './post-list.component.html',
  styleUrl: './post-list.component.scss'
})
export class PostListComponent {
  private readonly draftStore = inject(DraftStoreService);
  private readonly publisher = inject(PublisherService);
  private readonly refreshDraftsSubject = new Subject<void>();

  protected readonly drafts$: Observable<PostDraft[]> = this.refreshDraftsSubject.pipe(
    startWith(void 0),
    switchMap(() => this.draftStore.listDrafts())
  );
  protected feedback = '';
  protected publishError = '';

  protected deleteDraft(draft: PostDraft): void {
    const confirmed = confirm(`Delete "${draft.title}"?`);

    if (!confirmed) {
      return;
    }

    this.draftStore.deleteDraft(draft.id).subscribe(() => {
      this.feedback = 'Draft deleted.';
      this.refreshDraftsSubject.next();
    });
  }

  protected sandboxPublish(draft: PostDraft): void {
    this.feedback = '';
    this.publishError = '';

    if (draft.status === 'mock-published') {
      this.feedback = 'This draft is already sandbox-published. It was not posted to LinkedIn.';
      return;
    }

    this.publisher.publishDraft(draft).subscribe({
      next: (result) => {
        this.feedback = result.message;
      },
      error: () => {
        this.publishError = 'Sandbox publish failed. Nothing was posted to LinkedIn.';
      }
    });
  }

  protected getPreview(content: string): string {
    const normalizedContent = content.replace(/\s+/g, ' ').trim();
    return normalizedContent.length > 140 ? `${normalizedContent.slice(0, 140)}...` : normalizedContent;
  }

  protected getSeverity(status: PostStatus): 'info' | 'success' | 'warn' {
    if (status === 'mock-published') {
      return 'success';
    }

    return status === 'scheduled' ? 'warn' : 'info';
  }
}
