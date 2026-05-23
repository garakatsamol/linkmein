import { DraftImage } from '../models/draft-image.model';

export interface ImagePreviewOptions {
  acceptedMimeTypes: string[];
  maxSizeBytes: number;
}

export abstract class ImagePreviewService {
  abstract createPreview(file: File, options: ImagePreviewOptions): Promise<DraftImage>;
}
