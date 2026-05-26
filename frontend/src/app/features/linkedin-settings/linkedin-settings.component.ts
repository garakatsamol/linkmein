import { DatePipe } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { DividerModule } from 'primeng/divider';
import { MessageModule } from 'primeng/message';
import { TagModule } from 'primeng/tag';
import { finalize, take, timeout } from 'rxjs';

import { environment } from '../../../environments/environment';
import { AppearanceMode, AppearanceService } from '../../core/appearance/appearance.service';
import { ApiLinkedInStatus } from '../../core/api/models/api-linkedin-status.model';
import { ApiLinkedInService } from '../../core/services/api-linkedin.service';
import { resolveStorageMode, StorageMode, STORAGE_MODE_OVERRIDE_KEY } from '../../core/storage/storage-mode.config';

const LINKEDIN_STATUS_TIMEOUT_MS = 10000;

@Component({
  selector: 'app-linkedin-settings',
  imports: [ButtonModule, CardModule, DatePipe, DividerModule, MessageModule, TagModule],
  templateUrl: './linkedin-settings.component.html',
  styleUrl: './linkedin-settings.component.scss'
})
export class LinkedinSettingsComponent implements OnInit {
  private readonly appearance = inject(AppearanceService);
  private readonly linkedInApi = inject(ApiLinkedInService);
  private readonly changeDetector = inject(ChangeDetectorRef);

  protected appearanceMode = this.appearance.getCurrentAppearance();
  protected readonly configuredStorageMode = environment.storageMode;
  protected readonly effectiveStorageMode = resolveStorageMode(environment.storageMode);
  protected readonly storageModeOverride = this.getStorageModeOverride();
  protected connectionError = '';
  protected connectionFeedback = '';
  protected isDisconnecting = false;
  protected isLoadingConnection = true;
  protected linkedInStatus: ApiLinkedInStatus | null = null;

  ngOnInit(): void {
    this.loadLinkedInStatus();
  }

  protected connectLinkedIn(): void {
    if (typeof location !== 'undefined') {
      location.href = this.linkedInApi.getOAuthStartUrl();
    }
  }

  protected disconnectLinkedIn(): void {
    if (this.isDisconnecting) {
      return;
    }

    this.connectionError = '';
    this.connectionFeedback = '';
    this.isDisconnecting = true;
    this.refreshView();

    this.linkedInApi.disconnectLinkedIn().subscribe({
      next: () => {
        this.connectionFeedback = 'LinkedIn disconnected. Tokens remain server-side and are no longer active for this app.';
        this.isDisconnecting = false;
        this.refreshView();
        this.loadLinkedInStatus();
      },
      error: () => {
        this.connectionError = 'Unable to disconnect LinkedIn right now.';
        this.isDisconnecting = false;
        this.refreshView();
      }
    });
  }

  protected getConnectionSeverity(): 'success' | 'secondary' | 'warn' {
    if (this.connectionError) {
      return 'warn';
    }

    if (this.isLoadingConnection) {
      return 'warn';
    }

    return this.linkedInStatus?.connected ? 'success' : 'secondary';
  }

  protected getConnectionLabel(): string {
    if (this.connectionError) {
      return 'Status unavailable';
    }

    if (this.isLoadingConnection) {
      return 'Checking';
    }

    return this.linkedInStatus?.connected ? 'Connected' : 'Disconnected';
  }

  protected getLinkedInApiAccessLabel(): string {
    if (this.connectionError) {
      return 'Status unavailable';
    }

    if (this.isLoadingConnection) {
      return 'Checking';
    }

    return this.linkedInStatus?.connected ? 'Connected' : 'Not connected';
  }

  protected getLinkedInApiAccessSeverity(): 'success' | 'secondary' | 'warn' {
    if (this.connectionError || this.isLoadingConnection) {
      return 'warn';
    }

    return this.linkedInStatus?.connected ? 'success' : 'secondary';
  }

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

  protected useAppearance(mode: AppearanceMode): void {
    this.appearance.setAppearance(mode);
    this.appearanceMode = mode;
    this.refreshView();
  }

  protected isAppearanceActive(mode: AppearanceMode): boolean {
    return this.appearanceMode === mode;
  }

  protected getAppearanceButtonSeverity(mode: AppearanceMode): 'secondary' | undefined {
    return this.isAppearanceActive(mode) ? undefined : 'secondary';
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

  private loadLinkedInStatus(): void {
    this.connectionError = '';
    this.isLoadingConnection = true;

    this.linkedInApi
      .getLinkedInStatus()
      .pipe(
        take(1),
        timeout({ first: LINKEDIN_STATUS_TIMEOUT_MS }),
        finalize(() => {
          this.isLoadingConnection = false;
          this.refreshView();
        })
      )
      .subscribe({
        next: (status) => {
          this.linkedInStatus = status;
        },
        error: () => {
          this.linkedInStatus = null;
          this.connectionError = 'Unable to load LinkedIn connection status from the backend.';
        }
      });
  }

  private refreshView(): void {
    this.changeDetector.detectChanges();
  }

  private reload(): void {
    if (typeof location !== 'undefined') {
      location.reload();
    }
  }
}
