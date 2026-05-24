export interface ApiLinkedInStatus {
  connected: boolean;
  displayName?: string | null;
  email?: string | null;
  connectedAt?: string | null;
  accessTokenExpiresAt?: string | null;
  scopes: string[];
  message?: string | null;
}
