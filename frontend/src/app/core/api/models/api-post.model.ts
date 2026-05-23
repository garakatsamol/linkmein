export type ApiPostStatus = 'Draft' | 'Scheduled' | 'Publishing' | 'Published' | 'Failed';

export interface ApiPostDto {
  id: string;
  title: string;
  content: string;
  status: ApiPostStatus;
  scheduledFor?: string | null;
  publishedAt?: string | null;
  linkedInPostId?: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface CreateApiPostRequest {
  title: string;
  content: string;
  scheduledFor?: string;
}

export interface UpdateApiPostRequest {
  title: string;
  content: string;
  scheduledFor?: string;
}
