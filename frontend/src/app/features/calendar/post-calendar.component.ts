import { AsyncPipe, DatePipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { MessageModule } from 'primeng/message';
import { TagModule } from 'primeng/tag';
import { BehaviorSubject, Observable, combineLatest, map } from 'rxjs';

import { PostDraft, PostStatus } from '../../core/models/post-draft.model';
import { DraftStoreService } from '../../core/services/draft-store.service';

type StatusFilter = 'all' | PostStatus;

interface CalendarDay {
  date: Date;
  dateKey: string;
  inCurrentMonth: boolean;
  isToday: boolean;
  posts: PostDraft[];
}

interface CalendarView {
  days: CalendarDay[];
  monthLabel: string;
  upcomingPosts: PostDraft[];
}

@Component({
  selector: 'app-post-calendar',
  imports: [AsyncPipe, ButtonModule, CardModule, DatePipe, MessageModule, RouterLink, TagModule],
  templateUrl: './post-calendar.component.html',
  styleUrl: './post-calendar.component.scss'
})
export class PostCalendarComponent {
  private readonly draftStore = inject(DraftStoreService);
  private readonly router = inject(Router);
  private readonly currentMonthSubject = new BehaviorSubject<Date>(this.startOfMonth(new Date()));
  private readonly statusFilterSubject = new BehaviorSubject<StatusFilter>('all');

  protected readonly weekdayLabels = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
  protected readonly statusFilters: { label: string; value: StatusFilter }[] = [
    { label: 'All', value: 'all' },
    { label: 'Draft', value: 'draft' },
    { label: 'Scheduled', value: 'scheduled' },
    { label: 'Mock-published', value: 'mock-published' }
  ];
  protected readonly selectedFilter$ = this.statusFilterSubject.asObservable();
  protected readonly view$: Observable<CalendarView> = combineLatest([
    this.draftStore.listDrafts(),
    this.currentMonthSubject,
    this.statusFilterSubject
  ]).pipe(map(([drafts, currentMonth, filter]) => this.buildCalendarView(drafts, currentMonth, filter)));

  protected previousMonth(): void {
    const month = this.currentMonthSubject.value;
    this.currentMonthSubject.next(new Date(month.getFullYear(), month.getMonth() - 1, 1));
  }

  protected nextMonth(): void {
    const month = this.currentMonthSubject.value;
    this.currentMonthSubject.next(new Date(month.getFullYear(), month.getMonth() + 1, 1));
  }

  protected setFilter(filter: StatusFilter): void {
    this.statusFilterSubject.next(filter);
  }

  protected createDraftForDay(date: Date): void {
    const scheduledDate = new Date(date.getFullYear(), date.getMonth(), date.getDate(), 9, 0, 0);
    void this.router.navigate(['/composer'], { queryParams: { scheduledFor: scheduledDate.toISOString() } });
  }

  protected getSeverity(status: PostStatus): 'info' | 'success' | 'warn' {
    if (status === 'mock-published') {
      return 'success';
    }

    return status === 'scheduled' ? 'warn' : 'info';
  }

  protected hasVisiblePosts(days: CalendarDay[]): boolean {
    return days.some((day) => day.posts.length > 0);
  }

  private buildCalendarView(drafts: PostDraft[], currentMonth: Date, filter: StatusFilter): CalendarView {
    const filteredDrafts = this.filterDrafts(drafts, filter);
    const days = this.buildMonthDays(currentMonth, filteredDrafts);

    return {
      days,
      monthLabel: currentMonth.toLocaleDateString(undefined, { month: 'long', year: 'numeric' }),
      upcomingPosts: this.getUpcomingPosts(filteredDrafts)
    };
  }

  private buildMonthDays(currentMonth: Date, drafts: PostDraft[]): CalendarDay[] {
    const firstVisibleDay = new Date(currentMonth.getFullYear(), currentMonth.getMonth(), 1 - currentMonth.getDay());
    const todayKey = this.toDateKey(new Date());

    return Array.from({ length: 42 }, (_, index) => {
      const date = new Date(firstVisibleDay);
      date.setDate(firstVisibleDay.getDate() + index);
      const dateKey = this.toDateKey(date);

      return {
        date,
        dateKey,
        inCurrentMonth: date.getMonth() === currentMonth.getMonth(),
        isToday: dateKey === todayKey,
        posts: drafts.filter((draft) => this.toDateKey(this.getRelevantDate(draft)) === dateKey)
      };
    });
  }

  private getUpcomingPosts(drafts: PostDraft[]): PostDraft[] {
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    return drafts
      .filter((draft) => draft.scheduledFor && new Date(draft.scheduledFor) >= today)
      .sort((a, b) => new Date(a.scheduledFor ?? '').getTime() - new Date(b.scheduledFor ?? '').getTime());
  }

  private filterDrafts(drafts: PostDraft[], filter: StatusFilter): PostDraft[] {
    return filter === 'all' ? drafts : drafts.filter((draft) => draft.status === filter);
  }

  private getRelevantDate(draft: PostDraft): Date {
    return new Date(draft.scheduledFor ?? draft.createdAt ?? draft.updatedAt);
  }

  private startOfMonth(date: Date): Date {
    return new Date(date.getFullYear(), date.getMonth(), 1);
  }

  private toDateKey(date: Date): string {
    return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`;
  }
}
