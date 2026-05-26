import { Injectable } from '@angular/core';

export type AppearanceMode = 'light' | 'dark' | 'purple';

export const APPEARANCE_STORAGE_KEY = 'linkmein:appearance';

@Injectable({ providedIn: 'root' })
export class AppearanceService {
  private readonly classNames = ['linkmein-appearance-dark', 'linkmein-appearance-purple'];
  private currentAppearance: AppearanceMode = this.readStoredAppearance() ?? 'light';

  constructor() {
    this.applyAppearance(this.currentAppearance);
  }

  getCurrentAppearance(): AppearanceMode {
    return this.currentAppearance;
  }

  setAppearance(mode: AppearanceMode): void {
    this.currentAppearance = mode;
    this.writeStoredAppearance(mode);
    this.applyAppearance(mode);
  }

  private applyAppearance(mode: AppearanceMode): void {
    if (typeof document === 'undefined') {
      return;
    }

    const root = document.documentElement;
    root.classList.remove(...this.classNames);
    root.dataset['linkmeinAppearance'] = mode;

    if (mode === 'dark') {
      root.classList.add('linkmein-appearance-dark');
    }

    if (mode === 'purple') {
      root.classList.add('linkmein-appearance-purple');
    }
  }

  private readStoredAppearance(): AppearanceMode | null {
    let value: string | null | undefined;
    try {
      value = this.getStorage()?.getItem(APPEARANCE_STORAGE_KEY);
    } catch {
      value = null;
    }

    return value === 'light' || value === 'dark' || value === 'purple' ? value : null;
  }

  private writeStoredAppearance(mode: AppearanceMode): void {
    try {
      this.getStorage()?.setItem(APPEARANCE_STORAGE_KEY, mode);
    } catch {
      // Keep the visual change even if this browser blocks localStorage.
    }
  }

  private getStorage(): Storage | null {
    try {
      return typeof localStorage === 'undefined' ? null : localStorage;
    } catch {
      return null;
    }
  }
}
