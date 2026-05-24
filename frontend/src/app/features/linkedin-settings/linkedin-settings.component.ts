import { Component } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { DividerModule } from 'primeng/divider';
import { MessageModule } from 'primeng/message';
import { TagModule } from 'primeng/tag';

import { environment } from '../../../environments/environment';
import { resolveStorageMode, StorageMode, STORAGE_MODE_OVERRIDE_KEY } from '../../core/storage/storage-mode.config';

@Component({
  selector: 'app-linkedin-settings',
  imports: [ButtonModule, CardModule, DividerModule, MessageModule, TagModule],
  templateUrl: './linkedin-settings.component.html',
  styleUrl: './linkedin-settings.component.scss'
})
export class LinkedinSettingsComponent {
  protected readonly configuredStorageMode = environment.storageMode;
  protected readonly effectiveStorageMode = resolveStorageMode(environment.storageMode);
  protected readonly storageModeOverride = this.getStorageModeOverride();

  protected useStorageMode(mode: StorageMode): void {
    const storage = this.getStorage();

    storage?.setItem(STORAGE_MODE_OVERRIDE_KEY, mode);
    this.reload();
  }

  protected clearStorageModeOverride(): void {
    const storage = this.getStorage();

    storage?.removeItem(STORAGE_MODE_OVERRIDE_KEY);
    this.reload();
  }

  protected getStorageSeverity(mode: StorageMode): 'info' | 'warn' {
    return mode === 'api' ? 'warn' : 'info';
  }

  protected isStorageModeActive(mode: StorageMode): boolean {
    return this.effectiveStorageMode === mode;
  }

  protected getStorageButtonSeverity(mode: StorageMode): 'secondary' | 'warn' | undefined {
    if (this.isStorageModeActive(mode)) {
      return mode === 'api' ? 'warn' : undefined;
    }

    return 'secondary';
  }

  private getStorageModeOverride(): StorageMode | null {
    const value = this.getStorage()?.getItem(STORAGE_MODE_OVERRIDE_KEY);

    return value === 'api' || value === 'local' ? value : null;
  }

  private getStorage(): Storage | null {
    return typeof localStorage === 'undefined' ? null : localStorage;
  }

  private reload(): void {
    if (typeof location !== 'undefined') {
      location.reload();
    }
  }
}
