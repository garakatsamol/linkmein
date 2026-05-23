import { InjectionToken, Provider, inject } from '@angular/core';

import { ApiDraftStoreService } from '../services/api-draft-store.service';
import { DraftStoreService } from '../services/draft-store.service';
import { LocalDraftStoreService } from '../services/local-draft-store.service';

export type StorageMode = 'local' | 'api';

export const STORAGE_MODE = new InjectionToken<StorageMode>('linkmein.storageMode');
export const DEFAULT_STORAGE_MODE: StorageMode = 'local';

export function provideDraftStoreForStorageMode(mode: StorageMode = DEFAULT_STORAGE_MODE): Provider[] {
  return [
    { provide: STORAGE_MODE, useValue: mode },
    {
      provide: DraftStoreService,
      useFactory: () => {
        const storageMode = inject(STORAGE_MODE);

        return storageMode === 'api' ? inject(ApiDraftStoreService) : inject(LocalDraftStoreService);
      }
    }
  ];
}
