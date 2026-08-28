import { Injectable, computed, inject, signal } from '@angular/core';
import { ApiClient, ApiError } from './api-client';
import { clearCachedResources } from './cached-resource';
import type { CurrentUser } from './api.types';

export type SessionStatus = 'unknown' | 'signedIn' | 'signedOut';

/**
 * No passwords anywhere: a six-digit code by e-mail, then a cookie that
 * lasts long enough that most people never see the login screen again.
 *
 * Neither step ever says whether an address is known. Someone guessing
 * addresses learns nothing from the response, and the volunteer who
 * mistyped their own gets the same wording as everyone else.
 */
@Injectable({ providedIn: 'root' })
export class Session {
  private readonly api = inject(ApiClient);
  private readonly currentUser = signal<CurrentUser | null>(null);
  private readonly state = signal<SessionStatus>('unknown');
  private loading: Promise<void> | null = null;

  readonly user = this.currentUser.asReadonly();
  readonly status = this.state.asReadonly();
  readonly isSignedIn = computed(() => this.state() === 'signedIn');

  /** Resolves once we know whether there is a session. Safe to await twice. */
  async ensureLoaded(): Promise<void> {
    if (this.state() !== 'unknown') return;
    this.loading ??= this.load();
    await this.loading;
  }

  private async load(): Promise<void> {
    try {
      this.currentUser.set(await this.api.get<CurrentUser>('/api/me'));
      this.state.set('signedIn');
    } catch (cause) {
      if (cause instanceof ApiError) {
        this.state.set('signedOut');
      }
      // Offline with no answer either way: leave the status unknown so
      // the next attempt asks again instead of bouncing someone to the
      // login screen because a lift ate their signal.
    } finally {
      this.loading = null;
    }
  }

  async requestCode(email: string): Promise<void> {
    await this.api.post('/auth/login/request', { email });
  }

  async confirmCode(email: string, code: string): Promise<void> {
    await this.api.post('/auth/login/confirm', { email, code });
    this.state.set('unknown');
    await this.ensureLoaded();
  }

  async signOut(): Promise<void> {
    try {
      await this.api.post('/auth/logout');
    } finally {
      clearCachedResources();
      this.currentUser.set(null);
      this.state.set('signedOut');
      this.api.acknowledgeSessionExpiry();
    }
  }
}
