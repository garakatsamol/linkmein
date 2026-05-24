import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { API_BASE_URL, DEFAULT_API_BASE_URL } from '../api/api.config';
import { ApiLinkedInStatus } from '../api/models/api-linkedin-status.model';

@Injectable({ providedIn: 'root' })
export class ApiLinkedInService {
  private readonly http = inject(HttpClient);
  private readonly apiBaseUrl = this.normalizeApiBaseUrl(inject(API_BASE_URL));
  private readonly linkedinUrl = `${this.apiBaseUrl}/api/linkedin`;

  getLinkedInStatus(): Observable<ApiLinkedInStatus> {
    return this.http.get<ApiLinkedInStatus>(`${this.linkedinUrl}/status`);
  }

  disconnectLinkedIn(): Observable<void> {
    return this.http.post<void>(`${this.linkedinUrl}/disconnect`, {});
  }

  getOAuthStartUrl(): string {
    return `${this.linkedinUrl}/oauth/start`;
  }

  private normalizeApiBaseUrl(apiBaseUrl: string): string {
    const trimmedUrl = apiBaseUrl.trim();
    const absoluteUrl = trimmedUrl.startsWith('http://') || trimmedUrl.startsWith('https://') ? trimmedUrl : DEFAULT_API_BASE_URL;

    return absoluteUrl.replace(/\/+$/, '');
  }
}
