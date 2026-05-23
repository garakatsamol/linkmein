import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, map, of, throwError } from 'rxjs';

import { API_BASE_URL } from '../api/api.config';
import {
  mapApiPostToPostDraft,
  mapDraftPayloadToCreateApiPostRequest,
  mapDraftPayloadToUpdateApiPostRequest
} from '../api/post-api.mapper';
import { ApiPostDto } from '../api/models/api-post.model';
import { PostDraft } from '../models/post-draft.model';
import { DraftPayload, DraftStoreService } from './draft-store.service';

@Injectable({ providedIn: 'root' })
export class ApiDraftStoreService extends DraftStoreService {
  private readonly http = inject(HttpClient);
  private readonly apiBaseUrl = inject(API_BASE_URL);
  private readonly postsUrl = `${this.apiBaseUrl}/api/posts`;

  override listDrafts(): Observable<PostDraft[]> {
    return this.http.get<ApiPostDto[]>(this.postsUrl).pipe(map((posts) => posts.map(mapApiPostToPostDraft)));
  }

  override getDraft(id: string): Observable<PostDraft | undefined> {
    return this.http.get<ApiPostDto>(`${this.postsUrl}/${id}`).pipe(
      map(mapApiPostToPostDraft),
      catchError((error: unknown) => {
        if (error instanceof HttpErrorResponse && error.status === 404) {
          return of(undefined);
        }

        return throwError(() => error);
      })
    );
  }

  override createDraft(payload: DraftPayload): Observable<PostDraft> {
    return this.http
      .post<ApiPostDto>(this.postsUrl, mapDraftPayloadToCreateApiPostRequest(payload))
      .pipe(map(mapApiPostToPostDraft));
  }

  override updateDraft(id: string, payload: DraftPayload): Observable<PostDraft> {
    return this.http
      .put<ApiPostDto>(`${this.postsUrl}/${id}`, mapDraftPayloadToUpdateApiPostRequest(payload))
      .pipe(map(mapApiPostToPostDraft));
  }

  override deleteDraft(id: string): Observable<void> {
    return this.http.delete<void>(`${this.postsUrl}/${id}`);
  }
}
