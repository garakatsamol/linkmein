import { Injectable } from '@angular/core';

import { DraftImage } from '../models/draft-image.model';
import { ImagePreviewError, ImagePreviewOptions, ImagePreviewService } from './image-preview.service';

@Injectable({ providedIn: 'root' })
export class LocalImagePreviewService extends ImagePreviewService {
  override createPreview(file: File, options: ImagePreviewOptions): Promise<DraftImage> {
    this.validateFile(file, options);

    return new Promise((resolve, reject) => {
      const reader = new FileReader();

      reader.onload = () => {
        resolve({
          id: crypto.randomUUID(),
          fileName: file.name,
          mimeType: file.type,
          sizeBytes: file.size,
          dataUrl: String(reader.result)
        });
      };
      reader.onerror = () => reject(new ImagePreviewError(`Could not read ${file.name}.`));
      reader.readAsDataURL(file);
    });
  }

  private validateFile(file: File, options: ImagePreviewOptions): void {
    if (!options.acceptedMimeTypes.includes(file.type)) {
      throw new ImagePreviewError(`${file.name} is not a supported image type.`);
    }

    if (file.size > options.maxSizeBytes) {
      throw new ImagePreviewError(`${file.name} is larger than ${this.formatBytes(options.maxSizeBytes)}.`);
    }
  }

  private formatBytes(bytes: number): string {
    return `${Math.round((bytes / 1024 / 1024) * 10) / 10} MB`;
  }
}
