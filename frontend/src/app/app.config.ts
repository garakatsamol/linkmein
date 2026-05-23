import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideRouter } from '@angular/router';
import Aura from '@primeuix/themes/aura';
import { providePrimeNG } from 'primeng/config';

import { routes } from './app.routes';
import { DraftStoreService } from './core/services/draft-store.service';
import { ImagePreviewService } from './core/services/image-preview.service';
import { LocalDraftStoreService } from './core/services/local-draft-store.service';
import { LocalImagePreviewService } from './core/services/local-image-preview.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideAnimationsAsync(),
    provideRouter(routes),
    { provide: DraftStoreService, useExisting: LocalDraftStoreService },
    { provide: ImagePreviewService, useExisting: LocalImagePreviewService },
    providePrimeNG({
      ripple: true,
      inputVariant: 'outlined',
      theme: {
        preset: Aura,
        options: {
          darkModeSelector: false
        }
      }
    })
  ]
};
