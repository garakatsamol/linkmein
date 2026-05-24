import { AsyncPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { ButtonModule } from 'primeng/button';
import { MessageModule } from 'primeng/message';
import { BehaviorSubject, Observable, finalize, map, shareReplay, switchMap, take } from 'rxjs';

import { mapDraftToLinkedinPreview } from '../../core/mappers/linkedin-preview.mapper';
import { LinkedinPreview } from '../../core/models/linkedin-preview.model';
import { PostDraft } from '../../core/models/post-draft.model';
import { ApiPostPublisherService } from '../../core/services/api-post-publisher.service';
import { DraftStoreService } from '../../core/services/draft-store.service';
import { PublisherService } from '../../core/services/publisher.service';
import { STORAGE_MODE } from '../../core/storage/storage-mode.config';
import { LinkedinPreviewCardComponent } from '../../shared/components/linkedin-preview-card/linkedin-preview-card.component';

@Component({
  selector: 'app-post-preview',
  imports: [AsyncPipe, ButtonModule, LinkedinPreviewCardComponent, MessageModule, RouterLink],
  templateUrl: './post-preview.component.html',
  styleUrl: './post-preview.component.scss'
})
export class PostPreviewComponent {
  private readonly apiPublisher = inject(ApiPostPublisherService);
  private readonly draftStore = inject(DraftStoreService);
  private readonly publisher = inject(PublisherService);
  private readonly route = inject(ActivatedRoute);
  private readonly storageMode = inject(STORAGE_MODE);
  private readonly refreshDraft$ = new BehaviorSubject<void>(void 0);

  protected readonly draftId = this.route.snapshot.paramMap.get('id') ?? '';
  protected feedback = '';
  protected isPublishingToLinkedIn = false;
  protected publishError = '';
  protected readonly draft$: Observable<PostDraft | undefined> = this.refreshDraft$.pipe(
    switchMap(() => this.draftStore.getDraft(this.draftId)),
    shareReplay({ bufferSize: 1, refCount: true })
  );
  protected readonly preview$: Observable<LinkedinPreview | undefined> = this.draft$.pipe(
    map((draft) => (draft ? mapDraftToLinkedinPreview(draft) : undefined))
  );

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
        this.refreshDraft();
      },
      error: () => {
        this.publishError = 'Sandbox publish failed. Nothing was posted to LinkedIn.';
      }
    });
  }

  protected publishToLinkedIn(): void {
    this.feedback = '';
    this.publishError = '';

    if (!this.isApiMode()) {
      this.publishError = 'Real LinkedIn publishing requires API storage mode and a backend-saved post.';
      return;
    }

    if (this.isPublishingToLinkedIn) {
      return;
    }

    this.isPublishingToLinkedIn = true;
    this.apiPublisher
      .publishPost(this.draftId)
      .pipe(
        take(1),
        finalize(() => {
          this.isPublishingToLinkedIn = false;
        })
      )
      .subscribe({
        next: (result) => {
          this.feedback = result.message || 'Published to LinkedIn. Images were not published.';
          this.refreshDraft();
        },
        error: (error: unknown) => {
          this.publishError = this.getPublishErrorMessage(error);
        }
      });
  }

  protected getPublishedAtMessage(draft: PostDraft): string {
    if (this.isApiMode()) {
      return `Published to LinkedIn ${this.formatDate(draft.mockPublishedAt)}. Text-only publish; images were not published.`;
    }

    return `Sandbox published ${this.formatDate(draft.mockPublishedAt)}. Not posted to LinkedIn.`;
  }

  protected isApiMode(): boolean {
    return this.storageMode === 'api';
  }

  private formatDate(value: string | undefined): string {
    return value ? new Date(value).toLocaleString() : '';
  }

  private getPublishErrorMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      const responseMessage = typeof error.error?.message === 'string' ? error.error.message : '';

      if (responseMessage) {
        return responseMessage;
      }
    }

    return 'LinkedIn publish failed. No images were published.';
  }

  private refreshDraft(): void {
    this.refreshDraft$.next();
  }
}
