import { InjectionToken, Provider, inject } from '@angular/core';

import { ApiDraftStoreService } from '../services/api-draft-store.service';
import { DraftStoreService } from '../services/draft-store.service';
import { LocalDraftStoreService } from '../services/local-draft-store.service';

export type StorageMode = 'local' | 'api';

export const STORAGE_MODE = new InjectionToken<StorageMode>('linkmein.storageMode');
export const DEFAULT_STORAGE_MODE: StorageMode = 'local';
export const STORAGE_MODE_OVERRIDE_KEY = 'linkmein:storageMode';

export function provideDraftStoreForStorageMode(mode: StorageMode = DEFAULT_STORAGE_MODE): Provider[] {
  return [
    { provide: STORAGE_MODE, useFactory: () => resolveStorageMode(mode) },
    {
      provide: DraftStoreService,
      useFactory: () => {
        const storageMode = inject(STORAGE_MODE);

        return storageMode === 'api' ? inject(ApiDraftStoreService) : inject(LocalDraftStoreService);
      }
    }
  ];
}

export function resolveStorageMode(defaultMode: StorageMode = DEFAULT_STORAGE_MODE): StorageMode {
  const storage = getBrowserStorage();
  const override = storage?.getItem(STORAGE_MODE_OVERRIDE_KEY);

  return override === 'api' || override === 'local' ? override : defaultMode;
}

function getBrowserStorage(): Storage | null {
  return typeof localStorage === 'undefined' ? null : localStorage;
}
