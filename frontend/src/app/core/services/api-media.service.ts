import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { API_BASE_URL } from '../api/api.config';
import { ApiPostMediaDto } from '../api/models/api-post-media.model';

@Injectable({ providedIn: 'root' })
export class ApiMediaService {
  private readonly http = inject(HttpClient);
  private readonly apiBaseUrl = inject(API_BASE_URL);

  listMedia(postId: string): Observable<ApiPostMediaDto[]> {
    return this.http.get<ApiPostMediaDto[]>(this.getMediaUrl(postId));
  }

  uploadMedia(postId: string, file: File): Observable<ApiPostMediaDto> {
    const formData = new FormData();
    formData.append('file', file);

    return this.http.post<ApiPostMediaDto>(this.getMediaUrl(postId), formData);
  }

  deleteMedia(postId: string, mediaId: string): Observable<void> {
    return this.http.delete<void>(`${this.getMediaUrl(postId)}/${mediaId}`);
  }

  getMediaContentUrl(postId: string, mediaId: string): string {
    return `${this.getMediaUrl(postId)}/${mediaId}/content`;
  }

  private getMediaUrl(postId: string): string {
    return `${this.apiBaseUrl}/api/posts/${postId}/media`;
  }
}
