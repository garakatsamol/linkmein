import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { MessageModule } from 'primeng/message';
import { TextareaModule } from 'primeng/textarea';
import { finalize, take, timeout } from 'rxjs';

import { DraftImage } from '../../core/models/draft-image.model';
import { PostDraft } from '../../core/models/post-draft.model';
import { DraftStoreService } from '../../core/services/draft-store.service';
import { ImagePreviewService } from '../../core/services/image-preview.service';

const ACCEPTED_IMAGE_TYPES = ['image/jpeg', 'image/png', 'image/webp'];
const MAX_IMAGE_COUNT = 4;
const MAX_IMAGE_SIZE_BYTES = 4 * 1024 * 1024;
const SAVE_TIMEOUT_MS = 60000;

@Component({
  selector: 'app-post-composer',
  imports: [ButtonModule, CardModule, InputTextModule, MessageModule, ReactiveFormsModule, RouterLink, TextareaModule],
  templateUrl: './post-composer.component.html',
  styleUrl: './post-composer.component.scss'
})
export class PostComposerComponent implements OnInit {
  private readonly draftStore = inject(DraftStoreService);
  private readonly formBuilder = inject(FormBuilder);
  private readonly imagePreview = inject(ImagePreviewService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly changeDetector = inject(ChangeDetectorRef);

  protected draftId: string | null = null;
  protected feedback = '';
  protected images: DraftImage[] = [];
  protected imageValidationMessages: string[] = [];
  protected isSaving = false;
  protected loadError = '';
  private removedBackendImageIds: string[] = [];
  protected readonly acceptedImageTypes = ACCEPTED_IMAGE_TYPES.join(',');
  protected readonly maxImageCount = MAX_IMAGE_COUNT;
  protected readonly maxImageSizeLabel = '4 MB';

  protected readonly form = this.formBuilder.nonNullable.group({
    title: ['', Validators.required],
    content: ['', Validators.required],
    scheduledFor: ['']
  });

  ngOnInit(): void {
    this.draftId = this.route.snapshot.paramMap.get('id');

    if (!this.draftId) {
      const scheduledFor = this.route.snapshot.queryParamMap.get('scheduledFor');

      if (scheduledFor) {
        this.form.patchValue({ scheduledFor: this.toDateTimeLocalValue(scheduledFor) });
      }

      return;
    }

    this.draftStore.getDraft(this.draftId).subscribe((draft) => {
      if (!draft) {
        this.loadError = 'Draft not found.';
        this.refreshView();
        return;
      }

      this.patchForm(draft);
    });
  }

  protected saveDraft(): void {
    if (this.isSaving) {
      return;
    }

    this.feedback = '';
    this.loadError = '';

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const payload = {
      title: this.form.controls.title.value,
      content: this.form.controls.content.value,
      images: this.images,
      removedImageIds: this.removedBackendImageIds,
      scheduledFor: this.form.controls.scheduledFor.value || undefined
    };
    const request$ = this.draftId
      ? this.draftStore.updateDraft(this.draftId, payload)
      : this.draftStore.createDraft(payload);

    this.isSaving = true;

    request$.pipe(take(1), timeout({ first: SAVE_TIMEOUT_MS }), finalize(() => this.finishSaving())).subscribe({
      next: (draft) => {
        this.draftId = draft.id;
        this.images = draft.images;
        this.removedBackendImageIds = [];
        this.feedback = 'Draft saved.';
        this.refreshView();
        void this.router.navigate(['/composer', draft.id], { replaceUrl: true });
      },
      error: () => {
        this.loadError = 'Unable to save this draft.';
        this.refreshView();
      }
    });
  }

  protected cancel(): void {
    void this.router.navigateByUrl('/posts');
  }

  protected async addImages(event: Event): Promise<void> {
    this.imageValidationMessages = [];
    const input = event.target as HTMLInputElement;
    const selectedFiles = Array.from(input.files ?? []);

    input.value = '';

    for (const file of selectedFiles) {
      if (this.images.length >= MAX_IMAGE_COUNT) {
        this.imageValidationMessages.push(`You can attach up to ${MAX_IMAGE_COUNT} images per draft.`);
        this.refreshView();
        break;
      }

      try {
        const preview = await this.imagePreview.createPreview(file, {
          acceptedMimeTypes: ACCEPTED_IMAGE_TYPES,
          maxSizeBytes: MAX_IMAGE_SIZE_BYTES
        });
        this.images = [...this.images, preview];
        this.refreshView();
      } catch (error) {
        this.imageValidationMessages.push(error instanceof Error ? error.message : `Could not read ${file.name}.`);
        this.refreshView();
      }
    }
  }

  protected removeImage(imageId: string): void {
    const image = this.images.find((item) => item.id === imageId);

    if (image && !this.isLocalImage(image)) {
      this.removedBackendImageIds = [...this.removedBackendImageIds, image.id];
    }

    this.images = this.images.filter((image) => image.id !== imageId);
    this.imageValidationMessages = [];
    this.refreshView();
  }

  protected formatImageSize(sizeBytes: number): string {
    return `${Math.round((sizeBytes / 1024 / 1024) * 10) / 10} MB`;
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
    this.images = draft.images;
    this.removedBackendImageIds = [];
    this.refreshView();
  }

  private finishSaving(): void {
    this.isSaving = false;
    this.refreshView();
  }

  private isLocalImage(image: DraftImage): boolean {
    return image.dataUrl.startsWith('data:');
  }

  private refreshView(): void {
    this.changeDetector.detectChanges();
  }

  private toDateTimeLocalValue(value: string): string {
    const date = new Date(value);

    if (Number.isNaN(date.getTime())) {
      return value;
    }

    return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(
      2,
      '0'
    )}T${String(date.getHours()).padStart(2, '0')}:${String(date.getMinutes()).padStart(2, '0')}`;
  }
}
