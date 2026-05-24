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
      .pipe(
        switchMap((post) =>
          this.uploadLocalImages(post.id, payload.images).pipe(switchMap(() => this.getApiPostWithMedia(post.id)))
        )
      );
  }

  override updateDraft(id: string, payload: DraftPayload): Observable<PostDraft> {
    return this.http
      .put<ApiPostDto>(`${this.postsUrl}/${id}`, mapDraftPayloadToUpdateApiPostRequest(payload))
      .pipe(
        switchMap(() => this.deleteRemovedImages(id, payload.removedImageIds)),
        switchMap(() => this.uploadLocalImages(id, payload.images)),
        switchMap(() => this.getApiPostWithMedia(id))
      );
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

  private getApiPostWithMedia(id: string): Observable<PostDraft> {
    return this.http.get<ApiPostDto>(`${this.postsUrl}/${id}`).pipe(switchMap((post) => this.mapApiPostWithMedia(post)));
  }

  private uploadLocalImages(postId: string, images: DraftImage[] | undefined): Observable<unknown> {
    const localImageFiles = (images ?? [])
      .filter((image) => image.dataUrl.startsWith('data:'))
      .map((image) => this.dataUrlToFile(image));

    if (localImageFiles.length === 0) {
      return of(null);
    }

    return forkJoin(localImageFiles.map((file) => this.apiMedia.uploadMedia(postId, file)));
  }

  private deleteRemovedImages(postId: string, imageIds: string[] | undefined): Observable<unknown> {
    const uniqueImageIds = [...new Set(imageIds ?? [])];

    if (uniqueImageIds.length === 0) {
      return of(null);
    }

    return forkJoin(uniqueImageIds.map((imageId) => this.apiMedia.deleteMedia(postId, imageId)));
  }

  private dataUrlToFile(image: DraftImage): File {
    const [, encodedContent = ''] = image.dataUrl.split(',', 2);
    const binaryContent = atob(encodedContent);
    const bytes = new Uint8Array(binaryContent.length);

    for (let index = 0; index < binaryContent.length; index++) {
      bytes[index] = binaryContent.charCodeAt(index);
    }

    return new File([bytes], image.fileName, { type: image.mimeType });
  }
}
