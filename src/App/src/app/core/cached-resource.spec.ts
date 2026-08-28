import { describe, expect, it, beforeEach, vi } from 'vitest';
import { ApiClient, ApiError, OfflineError } from './api-client';
import { CachedResource, clearCachedResources } from './cached-resource';

/**
 * The distinction this file exists for: an unreachable server falls back
 * to the stored copy, an answering server does not. Getting that
 * backwards would either show a volunteer a day overview from this
 * morning as if it were current, or show an empty screen in a basement.
 */
describe('CachedResource', () => {
  beforeEach(() => {
    localStorage.clear();
  });

  function resourceWith(get: () => Promise<unknown>) {
    const api = { get: vi.fn(get) } as unknown as ApiClient;
    return { api, resource: new CachedResource<string>(api, '/api/today') };
  }

  it('serves what the server answered', async () => {
    const { resource } = resourceWith(async () => 'fresh');
    await resource.reload();

    expect(resource.value()).toBe('fresh');
    expect(resource.status()).toBe('ready');
    expect(resource.fetchedAt()).not.toBeNull();
  });

  it('falls back to the stored copy when the server cannot be reached', async () => {
    await resourceWith(async () => 'stored').resource.reload();

    const { resource } = resourceWith(async () => {
      throw new OfflineError('geen verbinding');
    });
    await resource.reload();

    expect(resource.value()).toBe('stored');
    expect(resource.status()).toBe('stale');
    expect(resource.isStale()).toBe(true);
  });

  it('treats an answer from the server as a real error, cache or not', async () => {
    await resourceWith(async () => 'stored').resource.reload();

    const { resource } = resourceWith(async () => {
      throw new ApiError(500);
    });
    await resource.reload();

    expect(resource.status()).toBe('error');
  });

  it('reports an error rather than an empty screen when there is nothing stored', async () => {
    const { resource } = resourceWith(async () => {
      throw new OfflineError('geen verbinding');
    });
    await resource.reload();

    expect(resource.value()).toBeNull();
    expect(resource.status()).toBe('error');
  });

  it('forgets everything on sign-out, because it holds phone numbers', async () => {
    await resourceWith(async () => 'stored').resource.reload();
    clearCachedResources();

    const { resource } = resourceWith(async () => {
      throw new OfflineError('geen verbinding');
    });
    await resource.reload();

    expect(resource.value()).toBeNull();
  });
});
