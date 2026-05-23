import { AsyncPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { MessageModule } from 'primeng/message';
import { Observable, map } from 'rxjs';

import { mapDraftToLinkedinPreview } from '../../core/mappers/linkedin-preview.mapper';
import { LinkedinPreview } from '../../core/models/linkedin-preview.model';
import { DraftStoreService } from '../../core/services/draft-store.service';
import { LinkedinPreviewCardComponent } from '../../shared/components/linkedin-preview-card/linkedin-preview-card.component';

@Component({
  selector: 'app-post-preview',
  imports: [AsyncPipe, ButtonModule, LinkedinPreviewCardComponent, MessageModule, RouterLink],
  templateUrl: './post-preview.component.html',
  styleUrl: './post-preview.component.scss'
})
export class PostPreviewComponent {
  private readonly draftStore = inject(DraftStoreService);
  private readonly route = inject(ActivatedRoute);

  protected readonly draftId = this.route.snapshot.paramMap.get('id') ?? '';
  protected readonly preview$: Observable<LinkedinPreview | undefined> = this.draftStore
    .getDraft(this.draftId)
    .pipe(map((draft) => (draft ? mapDraftToLinkedinPreview(draft) : undefined)));
}
