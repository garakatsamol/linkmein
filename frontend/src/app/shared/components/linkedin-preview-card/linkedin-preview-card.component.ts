import { DatePipe } from '@angular/common';
import { Component, input } from '@angular/core';
import { CardModule } from 'primeng/card';
import { DividerModule } from 'primeng/divider';
import { TagModule } from 'primeng/tag';

import { LinkedinPreview } from '../../../core/models/linkedin-preview.model';
import { PostStatus } from '../../../core/models/post-draft.model';

@Component({
  selector: 'app-linkedin-preview-card',
  imports: [CardModule, DatePipe, DividerModule, TagModule],
  templateUrl: './linkedin-preview-card.component.html',
  styleUrl: './linkedin-preview-card.component.scss'
})
export class LinkedinPreviewCardComponent {
  readonly preview = input.required<LinkedinPreview>();

  protected getSeverity(status: PostStatus): 'info' | 'success' | 'warn' {
    if (status === 'mock-published') {
      return 'success';
    }

    return status === 'scheduled' ? 'warn' : 'info';
  }
}
