import { InjectionToken } from '@angular/core';

export const API_BASE_URL = new InjectionToken<string>('linkmein.apiBaseUrl');

export const DEFAULT_API_BASE_URL = 'https://localhost:7001';
