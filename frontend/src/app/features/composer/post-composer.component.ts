import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { MessageModule } from 'primeng/message';
import { TextareaModule } from 'primeng/textarea';

import { PostDraft } from '../../core/models/post-draft.model';
import { DraftStoreService } from '../../core/services/draft-store.service';

@Component({
  selector: 'app-post-composer',
  imports: [ButtonModule, CardModule, InputTextModule, MessageModule, ReactiveFormsModule, TextareaModule],
  templateUrl: './post-composer.component.html',
  styleUrl: './post-composer.component.scss'
})
export class PostComposerComponent implements OnInit {
  private readonly draftStore = inject(DraftStoreService);
  private readonly formBuilder = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  protected draftId: string | null = null;
  protected feedback = '';
  protected loadError = '';

  protected readonly form = this.formBuilder.nonNullable.group({
    title: ['', Validators.required],
    content: ['', Validators.required],
    scheduledFor: ['']
  });

  ngOnInit(): void {
    this.draftId = this.route.snapshot.paramMap.get('id');

    if (!this.draftId) {
      return;
    }

    this.draftStore.getDraft(this.draftId).subscribe((draft) => {
      if (!draft) {
        this.loadError = 'Draft not found.';
        return;
      }

      this.patchForm(draft);
    });
  }

  protected saveDraft(): void {
    this.feedback = '';
    this.loadError = '';

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const payload = {
      title: this.form.controls.title.value,
      content: this.form.controls.content.value,
      scheduledFor: this.form.controls.scheduledFor.value || undefined
    };
    const request$ = this.draftId
      ? this.draftStore.updateDraft(this.draftId, payload)
      : this.draftStore.createDraft(payload);

    request$.subscribe({
      next: () => {
        this.feedback = 'Draft saved.';
        void this.router.navigateByUrl('/posts');
      },
      error: () => {
        this.loadError = 'Unable to save this draft.';
      }
    });
  }

  protected cancel(): void {
    void this.router.navigateByUrl('/posts');
  }

  protected isInvalid(controlName: 'title' | 'content'): boolean {
    const control = this.form.controls[controlName];
    return control.invalid && (control.dirty || control.touched);
  }

  private patchForm(draft: PostDraft): void {
    this.form.patchValue({
      title: draft.title,
      content: draft.content,
      scheduledFor: draft.scheduledFor ?? ''
    });
  }
}
