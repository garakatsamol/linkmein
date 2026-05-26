import { Component, EventEmitter, Output, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { TextareaModule } from 'primeng/textarea';
import { CardModule } from 'primeng/card';
import { MessageModule } from 'primeng/message';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ApiAiService, GeneratePostSuggestionRequest, GeneratePostSuggestionResponse } from '../../core/services/api-ai.service';
import { finalize } from 'rxjs/operators';

export interface AiSuggestionSelection {
  suggestedTitle?: string;
  suggestedText: string;
}

@Component({
  selector: 'app-ai-assist-panel',
  standalone: true,
  imports: [CommonModule, FormsModule, ButtonModule, TextareaModule, CardModule, MessageModule, ProgressSpinnerModule],
  templateUrl: './ai-assist-panel.component.html',
  styleUrl: './ai-assist-panel.component.scss'
})
export class AiAssistPanelComponent {
  idea = '';
  tone = 'professional';
  tones = [
    { label: 'Professional', value: 'professional' },
    { label: 'Concise', value: 'concise' },
    { label: 'Technical', value: 'technical' },
    { label: 'Leadership', value: 'leadership' }
  ];
  loading = false;
  error = '';
  suggestion: GeneratePostSuggestionResponse | null = null;

  @Output() useSuggestion = new EventEmitter<AiSuggestionSelection>();

  constructor(
    private readonly aiService: ApiAiService,
    private readonly cdr: ChangeDetectorRef
  ) {}

  generateSuggestion() {
    this.error = '';
    this.suggestion = null;
    if (!this.idea.trim()) {
      this.error = 'Idea is required.';
      return;
    }
    const req: GeneratePostSuggestionRequest = {
      idea: this.idea.trim(),
      tone: this.tone || 'professional',
      language: 'English'
    };
    this.loading = true;
    this.aiService.generatePostSuggestion(req)
      .pipe(
        finalize(() => {
          this.loading = false;
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: (res: GeneratePostSuggestionResponse) => {
          this.suggestion = res;
          this.cdr.detectChanges();
        },
        error: (err) => {
          this.error = err?.error?.message || 'Failed to generate suggestion.';
          this.cdr.detectChanges();
        }
      });
  }

  handleUseSuggestion() {
    if (this.suggestion?.suggestedText) {
      this.useSuggestion.emit({
        suggestedTitle: this.suggestion.suggestedTitle,
        suggestedText: this.suggestion.suggestedText
      });
    }
  }
}
