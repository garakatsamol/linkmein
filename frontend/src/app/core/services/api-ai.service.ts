import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api/api.config';

export interface GeneratePostSuggestionRequest {
  idea: string;
  tone?: string;
  language?: string;
}

export interface GeneratePostSuggestionResponse {
  suggestedTitle?: string;
  suggestedText: string;
  message?: string;
}

@Injectable({ providedIn: 'root' })
export class ApiAiService {
  private readonly http = inject(HttpClient);
  private readonly apiBaseUrl = inject(API_BASE_URL);

  generatePostSuggestion(request: GeneratePostSuggestionRequest): Observable<GeneratePostSuggestionResponse> {
    return this.http.post<GeneratePostSuggestionResponse>(
      `${this.apiBaseUrl}/api/ai/post-suggestions`,
      request
    );
  }
}
