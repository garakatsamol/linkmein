import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { API_BASE_URL, DEFAULT_API_BASE_URL } from '../api/api.config';
import { ApiPublishPostResponse } from '../api/models/api-publish-post-response.model';

@Injectable({ providedIn: 'root' })
export class ApiPostPublisherService {
  private readonly http = inject(HttpClient);
  private readonly apiBaseUrl = this.normalizeApiBaseUrl(inject(API_BASE_URL));

  publishPost(postId: string): Observable<ApiPublishPostResponse> {
    return this.http.post<ApiPublishPostResponse>(`${this.apiBaseUrl}/api/posts/${postId}/publish`, {});
  }

  private normalizeApiBaseUrl(apiBaseUrl: string): string {
    const trimmedUrl = apiBaseUrl.trim();
    const absoluteUrl = trimmedUrl.startsWith('http://') || trimmedUrl.startsWith('https://') ? trimmedUrl : DEFAULT_API_BASE_URL;

    return absoluteUrl.replace(/\/+$/, '');
  }
}
