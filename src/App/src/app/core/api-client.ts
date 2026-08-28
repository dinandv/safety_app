import { Injectable, signal } from '@angular/core';

export class ApiError extends Error {
  constructor(
    readonly status: number,
    readonly reason?: string,
  ) {
    super(`API ${status}${reason ? ` (${reason})` : ''}`);
  }
}

/** Thrown when the request never reached the server. */
export class OfflineError extends Error {}

/**
 * Everything the app reads and writes goes through here.
 *
 * Same origin as the API, so the session cookie rides along on its own
 * and there is no token to keep anywhere. A 401 is not an error the
 * screens have to handle one by one — it flips {@link sessionExpired},
 * and the shell reacts to that.
 */
@Injectable({ providedIn: 'root' })
export class ApiClient {
  private readonly expired = signal(false);

  /** Goes true the moment any call comes back unauthenticated. */
  readonly sessionExpired = this.expired.asReadonly();

  acknowledgeSessionExpiry(): void {
    this.expired.set(false);
  }

  async get<T>(path: string, signal?: AbortSignal): Promise<T> {
    return this.send<T>('GET', path, undefined, signal);
  }

  async post<T>(path: string, body?: unknown, signal?: AbortSignal): Promise<T> {
    return this.send<T>('POST', path, body, signal);
  }

  private async send<T>(
    method: string,
    path: string,
    body: unknown,
    abort?: AbortSignal,
  ): Promise<T> {
    let response: Response;
    try {
      response = await fetch(path, {
        method,
        signal: abort,
        headers: body === undefined ? undefined : { 'Content-Type': 'application/json' },
        body: body === undefined ? undefined : JSON.stringify(body),
      });
    } catch (cause) {
      // fetch only rejects when the request never completed: no network,
      // DNS failure, the service worker having nothing cached. Anything
      // the server answered — including a 500 — lands below.
      if (abort?.aborted) throw cause;
      throw new OfflineError('Geen verbinding');
    }

    if (response.status === 401) {
      this.expired.set(true);
      throw new ApiError(401);
    }

    if (!response.ok) {
      throw new ApiError(response.status, await readReason(response));
    }

    if (response.status === 204) return undefined as T;
    return (await response.json()) as T;
  }
}

async function readReason(response: Response): Promise<string | undefined> {
  try {
    const body = await response.json();
    return typeof body?.reason === 'string' ? body.reason : undefined;
  } catch {
    return undefined;
  }
}
