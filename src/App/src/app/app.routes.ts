import { Routes } from '@angular/router';
import { signedInGuard, signedOutGuard } from './core/guards';

/**
 * Route paths are English like the rest of the code; only what the
 * volunteer reads is Dutch. In a installed PWA the address bar is hidden
 * anyway, so nothing is lost by keeping the two apart.
 */
export const routes: Routes = [
  {
    path: 'login',
    canActivate: [signedOutGuard],
    loadComponent: () => import('./features/login/login-page').then((m) => m.LoginPage),
  },
  {
    path: '',
    canActivate: [signedInGuard],
    loadComponent: () => import('./shell/shell').then((m) => m.Shell),
    children: [
      {
        path: 'today',
        title: 'Vandaag',
        loadComponent: () => import('./features/today/today-page').then((m) => m.TodayPage),
      },
      {
        path: 'shifts',
        title: 'Mijn diensten',
        loadComponent: () =>
          import('./features/my-shifts/my-shifts-page').then((m) => m.MyShiftsPage),
      },
      {
        path: 'open-calls',
        title: 'Open oproepen',
        loadComponent: () =>
          import('./features/open-calls/open-calls-page').then((m) => m.OpenCallsPage),
      },
      {
        path: 'info',
        title: 'Informatie',
        loadComponent: () => import('./features/info/info-page').then((m) => m.InfoPage),
      },
      {
        path: 'me',
        title: 'Ik',
        loadComponent: () => import('./features/profile/profile-page').then((m) => m.ProfilePage),
      },
      { path: '', pathMatch: 'full', redirectTo: 'today' },
    ],
  },
  { path: '**', redirectTo: '' },
];
