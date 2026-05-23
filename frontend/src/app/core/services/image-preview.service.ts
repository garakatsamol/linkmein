import { DraftImage } from '../models/draft-image.model';

export interface ImagePreviewOptions {
  acceptedMimeTypes: string[];
  maxSizeBytes: number;
}

export class ImagePreviewError extends Error {
  constructor(message: string) {
    super(message);
    this.name = 'ImagePreviewError';
  }
}

export abstract class ImagePreviewService {
  abstract createPreview(file: File, options: ImagePreviewOptions): Promise<DraftImage>;
}
