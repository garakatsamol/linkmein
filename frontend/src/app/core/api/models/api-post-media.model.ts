export interface ApiPostMediaDto {
  id: string;
  postId: string;
  fileName: string;
  contentType: string;
  sizeBytes: number;
  createdAt: string;
  linkedInAssetUrn?: string | null;
}
