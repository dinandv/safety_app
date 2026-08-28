import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { Session } from './session';

/**
 * The status starts as `unknown` and stays that way when the check
 * itself could not reach the server. An unreachable server is not proof
 * that a session expired, so an offline start lets the app through to
 * its cached screens rather than to a login form it cannot submit.
 */
export const signedInGuard: CanActivateFn = async () => {
  const session = inject(Session);
  const router = inject(Router);

  await session.ensureLoaded();
  if (session.status() === 'signedOut') return router.createUrlTree(['/login']);
  return true;
};

export const signedOutGuard: CanActivateFn = async () => {
  const session = inject(Session);
  const router = inject(Router);

  await session.ensureLoaded();
  if (session.status() === 'signedIn') return router.createUrlTree(['/']);
  return true;
};
