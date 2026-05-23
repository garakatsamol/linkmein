import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, forkJoin, map, of, switchMap, throwError } from 'rxjs';

import { API_BASE_URL } from '../api/api.config';
import { ApiPostMediaDto } from '../api/models/api-post-media.model';
import {
  mapApiPostToPostDraft,
  mapDraftPayloadToCreateApiPostRequest,
  mapDraftPayloadToUpdateApiPostRequest
} from '../api/post-api.mapper';
import { ApiPostDto } from '../api/models/api-post.model';
import { DraftImage } from '../models/draft-image.model';
import { PostDraft } from '../models/post-draft.model';
import { DraftPayload, DraftStoreService } from './draft-store.service';
import { ApiMediaService } from './api-media.service';

@Injectable({ providedIn: 'root' })
export class ApiDraftStoreService extends DraftStoreService {
  private readonly http = inject(HttpClient);
  private readonly apiBaseUrl = inject(API_BASE_URL);
  private readonly apiMedia = inject(ApiMediaService);
  private readonly postsUrl = `${this.apiBaseUrl}/api/posts`;

  override listDrafts(): Observable<PostDraft[]> {
    return this.http.get<ApiPostDto[]>(this.postsUrl).pipe(
      switchMap((posts) => {
        if (posts.length === 0) {
          return of([]);
        }

        return forkJoin(posts.map((post) => this.mapApiPostWithMedia(post)));
      })
    );
  }

  override getDraft(id: string): Observable<PostDraft | undefined> {
    return this.http.get<ApiPostDto>(`${this.postsUrl}/${id}`).pipe(
      switchMap((post) => this.mapApiPostWithMedia(post)),
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

  private mapApiPostWithMedia(post: ApiPostDto): Observable<PostDraft> {
    return this.apiMedia.listMedia(post.id).pipe(
      map((media) => ({
        ...mapApiPostToPostDraft(post),
        images: media.map((item) => this.mapApiMediaToDraftImage(post.id, item))
      }))
    );
  }

  private mapApiMediaToDraftImage(postId: string, media: ApiPostMediaDto): DraftImage {
    return {
      id: media.id,
      fileName: media.fileName,
      mimeType: media.contentType,
      sizeBytes: media.sizeBytes,
      dataUrl: this.apiMedia.getMediaContentUrl(postId, media.id)
    };
  }
}
