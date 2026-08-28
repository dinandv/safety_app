import { computed, signal } from '@angular/core';
import { ApiClient, OfflineError } from './api-client';

export type ResourceStatus = 'loading' | 'ready' | 'stale' | 'error';

const CACHE_PREFIX = 'bcc.cache.';

interface CacheEntry<T> {
  readonly storedAt: string;
  readonly value: T;
}

/**
 * A GET that keeps its last answer.
 *
 * The day overview and the contact card are read in places with no
 * signal, so "we could not reach the server" must never mean an empty
 * screen. When the request fails on the network the last stored answer
 * comes back with status `stale`, and the screen says how old it is —
 * never silently, because a day overview from this morning is missing
 * exactly the withdrawal you would want to know about.
 *
 * Anything the server actually answered is a real error and is shown as
 * one; only an unreachable server falls back.
 */
export class CachedResource<T> {
  private readonly state = signal<T | null>(null);
  private readonly stamp = signal<Date | null>(null);
  private readonly phase = signal<ResourceStatus>('loading');
  private readonly failure = signal<unknown>(null);
  private inFlight: AbortController | null = null;

  readonly value = this.state.asReadonly();
  readonly status = this.phase.asReadonly();
  readonly error = this.failure.asReadonly();

  /** When the data in {@link value} was fetched. Null while empty. */
  readonly fetchedAt = this.stamp.asReadonly();

  readonly isStale = computed(() => this.phase() === 'stale');

  constructor(
    private readonly api: ApiClient,
    private readonly path: string,
  ) {
    const cached = this.read();
    if (cached) {
      this.state.set(cached.value);
      this.stamp.set(new Date(cached.storedAt));
    }
  }

  async reload(): Promise<void> {
    this.inFlight?.abort();
    const controller = new AbortController();
    this.inFlight = controller;

    // Keep whatever is on screen while refreshing: a spinner over a
    // readable overview is a downgrade.
    if (this.state() === null) this.phase.set('loading');

    try {
      const value = await this.api.get<T>(this.path, controller.signal);
      if (controller.signal.aborted) return;
      this.state.set(value);
      this.stamp.set(new Date());
      this.phase.set('ready');
      this.failure.set(null);
      this.write(value);
    } catch (cause) {
      if (controller.signal.aborted) return;
      this.failure.set(cause);
      const offline = cause instanceof OfflineError;
      this.phase.set(offline && this.state() !== null ? 'stale' : 'error');
    } finally {
      if (this.inFlight === controller) this.inFlight = null;
    }
  }

  private read(): CacheEntry<T> | null {
    try {
      const raw = localStorage.getItem(CACHE_PREFIX + this.path);
      return raw ? (JSON.parse(raw) as CacheEntry<T>) : null;
    } catch {
      return null;
    }
  }

  private write(value: T): void {
    try {
      const entry: CacheEntry<T> = { storedAt: new Date().toISOString(), value };
      localStorage.setItem(CACHE_PREFIX + this.path, JSON.stringify(entry));
    } catch {
      // A full or blocked store costs us the offline copy, nothing else.
    }
  }
}

/**
 * Wipes every cached answer. Called on sign-out: the cache holds
 * colleagues' phone numbers, and those should not outlive the session on
 * a shared or lost phone.
 */
export function clearCachedResources(): void {
  try {
    for (const key of Object.keys(localStorage)) {
      if (key.startsWith(CACHE_PREFIX)) localStorage.removeItem(key);
    }
  } catch {
    // Nothing we can do, and nothing that should block signing out.
  }
}
