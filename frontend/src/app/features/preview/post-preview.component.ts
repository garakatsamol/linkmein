import { AsyncPipe, DatePipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { MessageModule } from 'primeng/message';
import { Observable, map } from 'rxjs';

import { mapDraftToLinkedinPreview } from '../../core/mappers/linkedin-preview.mapper';
import { LinkedinPreview } from '../../core/models/linkedin-preview.model';
import { PostDraft } from '../../core/models/post-draft.model';
import { DraftStoreService } from '../../core/services/draft-store.service';
import { PublisherService } from '../../core/services/publisher.service';
import { LinkedinPreviewCardComponent } from '../../shared/components/linkedin-preview-card/linkedin-preview-card.component';

@Component({
  selector: 'app-post-preview',
  imports: [AsyncPipe, ButtonModule, DatePipe, LinkedinPreviewCardComponent, MessageModule, RouterLink],
  templateUrl: './post-preview.component.html',
  styleUrl: './post-preview.component.scss'
})
export class PostPreviewComponent {
  private readonly draftStore = inject(DraftStoreService);
  private readonly publisher = inject(PublisherService);
  private readonly route = inject(ActivatedRoute);

  protected readonly draftId = this.route.snapshot.paramMap.get('id') ?? '';
  protected feedback = '';
  protected publishError = '';
  protected readonly draft$: Observable<PostDraft | undefined> = this.draftStore
    .listDrafts()
    .pipe(map((drafts) => drafts.find((draft) => draft.id === this.draftId)));
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
      },
      error: () => {
        this.publishError = 'Sandbox publish failed. Nothing was posted to LinkedIn.';
      }
    });
  }
}
