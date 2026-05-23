import { AsyncPipe, DatePipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { MessageModule } from 'primeng/message';
import { TagModule } from 'primeng/tag';
import { Observable } from 'rxjs';

import { PostDraft, PostStatus } from '../../core/models/post-draft.model';
import { DraftStoreService } from '../../core/services/draft-store.service';

@Component({
  selector: 'app-post-list',
  imports: [AsyncPipe, ButtonModule, CardModule, DatePipe, MessageModule, RouterLink, TagModule],
  templateUrl: './post-list.component.html',
  styleUrl: './post-list.component.scss'
})
export class PostListComponent {
  private readonly draftStore = inject(DraftStoreService);

  protected readonly drafts$: Observable<PostDraft[]> = this.draftStore.listDrafts();
  protected feedback = '';

  protected deleteDraft(draft: PostDraft): void {
    const confirmed = confirm(`Delete "${draft.title}"?`);

    if (!confirmed) {
      return;
    }

    this.draftStore.deleteDraft(draft.id).subscribe(() => {
      this.feedback = 'Draft deleted.';
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
