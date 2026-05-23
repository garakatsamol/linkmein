import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, of, throwError } from 'rxjs';

import { PostDraft } from '../models/post-draft.model';
import { DraftPayload, DraftStoreService } from './draft-store.service';

const STORAGE_KEY = 'linkmein:drafts:v1';

@Injectable({ providedIn: 'root' })
export class LocalDraftStoreService extends DraftStoreService {
  private readonly draftsSubject = new BehaviorSubject<PostDraft[]>(this.readDrafts());

  override listDrafts(): Observable<PostDraft[]> {
    return this.draftsSubject.asObservable();
  }

  override getDraft(id: string): Observable<PostDraft | undefined> {
    return of(this.draftsSubject.value.find((draft) => draft.id === id));
  }

  override createDraft(payload: DraftPayload): Observable<PostDraft> {
    const now = new Date().toISOString();
    const draft: PostDraft = {
      id: crypto.randomUUID(),
      title: payload.title.trim(),
      content: payload.content.trim(),
      images: payload.images ?? [],
      scheduledFor: payload.scheduledFor,
      status: payload.status ?? this.getStatus(payload.scheduledFor),
      createdAt: now,
      updatedAt: now
    };

    this.persist([...this.draftsSubject.value, draft]);
    return of(draft);
  }

  override updateDraft(id: string, payload: DraftPayload): Observable<PostDraft> {
    const currentDrafts = this.draftsSubject.value;
    const draftIndex = currentDrafts.findIndex((draft) => draft.id === id);

    if (draftIndex < 0) {
      return throwError(() => new Error('Draft not found.'));
    }

    const existingDraft = currentDrafts[draftIndex];
    const updatedDraft: PostDraft = {
      ...existingDraft,
      title: payload.title.trim(),
      content: payload.content.trim(),
      images: payload.images ?? existingDraft.images,
      mockPublishedAt: payload.mockPublishedAt ?? existingDraft.mockPublishedAt,
      scheduledFor: payload.scheduledFor,
      status: payload.status ?? this.getStatus(payload.scheduledFor),
      updatedAt: new Date().toISOString()
    };
    const updatedDrafts = currentDrafts.map((draft) => (draft.id === id ? updatedDraft : draft));

    this.persist(updatedDrafts);
    return of(updatedDraft);
  }

  override deleteDraft(id: string): Observable<void> {
    this.persist(this.draftsSubject.value.filter((draft) => draft.id !== id));
    return of(void 0);
  }

  private readDrafts(): PostDraft[] {
    const storage = this.getStorage();

    if (!storage) {
      return [];
    }

    const rawDrafts = storage.getItem(STORAGE_KEY);

    if (!rawDrafts) {
      return [];
    }

    try {
      const parsedDrafts = JSON.parse(rawDrafts) as PostDraft[];
      return Array.isArray(parsedDrafts) ? parsedDrafts.map((draft) => ({ ...draft, images: draft.images ?? [] })) : [];
    } catch {
      return [];
    }
  }

  private persist(drafts: PostDraft[]): void {
    const sortedDrafts = [...drafts].sort((a, b) => b.updatedAt.localeCompare(a.updatedAt));
    const storage = this.getStorage();

    if (storage) {
      storage.setItem(STORAGE_KEY, JSON.stringify(sortedDrafts));
    }

    this.draftsSubject.next(sortedDrafts);
  }

  private getStorage(): Storage | null {
    return typeof localStorage === 'undefined' ? null : localStorage;
  }

  private getStatus(scheduledFor: string | undefined): 'draft' | 'scheduled' {
    return scheduledFor ? 'scheduled' : 'draft';
  }
}
