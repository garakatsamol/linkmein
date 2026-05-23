import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideRouter } from '@angular/router';
import Aura from '@primeuix/themes/aura';
import { providePrimeNG } from 'primeng/config';

import { routes } from './app.routes';
import { environment } from '../environments/environment';
import { API_BASE_URL } from './core/api/api.config';
import { ImagePreviewService } from './core/services/image-preview.service';
import { LocalImagePreviewService } from './core/services/local-image-preview.service';
import { MockPublisherService } from './core/services/mock-publisher.service';
import { PublisherService } from './core/services/publisher.service';
import { provideDraftStoreForStorageMode } from './core/storage/storage-mode.config';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideHttpClient(),
    provideAnimationsAsync(),
    provideRouter(routes),
    { provide: API_BASE_URL, useValue: environment.apiBaseUrl },
    provideDraftStoreForStorageMode(environment.storageMode),
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
