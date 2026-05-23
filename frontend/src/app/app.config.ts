import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideRouter } from '@angular/router';
import Aura from '@primeuix/themes/aura';
import { providePrimeNG } from 'primeng/config';

import { routes } from './app.routes';
import { API_BASE_URL, DEFAULT_API_BASE_URL } from './core/api/api.config';
import { DraftStoreService } from './core/services/draft-store.service';
import { ImagePreviewService } from './core/services/image-preview.service';
import { LocalDraftStoreService } from './core/services/local-draft-store.service';
import { LocalImagePreviewService } from './core/services/local-image-preview.service';
import { MockPublisherService } from './core/services/mock-publisher.service';
import { PublisherService } from './core/services/publisher.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideHttpClient(),
    provideAnimationsAsync(),
    provideRouter(routes),
    { provide: API_BASE_URL, useValue: DEFAULT_API_BASE_URL },
    { provide: DraftStoreService, useExisting: LocalDraftStoreService },
    { provide: ImagePreviewService, useExisting: LocalImagePreviewService },
    { provide: PublisherService, useExisting: MockPublisherService },
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
