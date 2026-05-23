import { AsyncPipe, DatePipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { MessageModule } from 'primeng/message';
import { TagModule } from 'primeng/tag';
import { Observable, map } from 'rxjs';

import { PostDraft, PostStatus } from '../../core/models/post-draft.model';
import { DraftStoreService } from '../../core/services/draft-store.service';

interface DashboardMetric {
  label: string;
  value: number;
  icon: string;
}

interface DashboardView {
  metrics: DashboardMetric[];
  featuredPosts: PostDraft[];
}

@Component({
  selector: 'app-dashboard',
  imports: [AsyncPipe, ButtonModule, CardModule, DatePipe, MessageModule, RouterLink, TagModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent {
  private readonly draftStore = inject(DraftStoreService);

  protected readonly view$: Observable<DashboardView> = this.draftStore
    .listDrafts()
    .pipe(map((drafts) => this.buildView(drafts)));

  protected getSeverity(status: PostStatus): 'info' | 'success' | 'warn' {
    if (status === 'mock-published') {
      return 'success';
    }

    return status === 'scheduled' ? 'warn' : 'info';
  }

  private buildView(drafts: PostDraft[]): DashboardView {
    const upcoming = this.getUpcomingScheduled(drafts);
    const featuredPosts =
      upcoming.length > 0
        ? upcoming.slice(0, 5)
        : [...drafts].sort((a, b) => b.updatedAt.localeCompare(a.updatedAt)).slice(0, 5);

    return {
      metrics: [
        { label: 'Total drafts', value: drafts.length, icon: 'pi pi-file-edit' },
        { label: 'Scheduled posts', value: drafts.filter((draft) => draft.status === 'scheduled').length, icon: 'pi pi-calendar' },
        {
          label: 'Sandbox-published',
          value: drafts.filter((draft) => draft.status === 'mock-published').length,
          icon: 'pi pi-check-circle'
        },
        { label: 'Posts with images', value: drafts.filter((draft) => draft.images.length > 0).length, icon: 'pi pi-image' },
        { label: 'Upcoming scheduled', value: upcoming.length, icon: 'pi pi-clock' }
      ],
      featuredPosts
    };
  }

  private getUpcomingScheduled(drafts: PostDraft[]): PostDraft[] {
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    return drafts
      .filter((draft) => draft.scheduledFor && new Date(draft.scheduledFor) >= today)
      .sort((a, b) => new Date(a.scheduledFor ?? '').getTime() - new Date(b.scheduledFor ?? '').getTime());
  }
}
