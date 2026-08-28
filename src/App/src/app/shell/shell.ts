import { ChangeDetectionStrategy, Component, effect, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { ApiClient } from '../core/api-client';
import { Session } from '../core/session';
import { UiIcon, type IconName } from '../ui/icon';

interface Tab {
  readonly path: string;
  readonly label: string;
  readonly icon: IconName;
}

/**
 * Five tabs, and "Vandaag" first.
 *
 * The complaint that started this application is that nobody knows who
 * is on duty today, and the person asking usually has no shift
 * themselves. Opening on "Mijn diensten" would answer a question they
 * did not ask.
 */
const TABS: readonly Tab[] = [
  { path: 'today', label: 'Vandaag', icon: 'calendar-days' },
  { path: 'shifts', label: 'Diensten', icon: 'calendar-check' },
  { path: 'open-calls', label: 'Oproepen', icon: 'megaphone' },
  { path: 'info', label: 'Info', icon: 'book-open' },
  { path: 'me', label: 'Ik', icon: 'user' },
];

@Component({
  selector: 'app-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, UiIcon],
  templateUrl: './shell.html',
  styleUrl: './shell.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Shell {
  private readonly api = inject(ApiClient);
  private readonly router = inject(Router);
  private readonly session = inject(Session);

  readonly tabs = TABS;

  constructor() {
    // A session can expire while the app is open — the cookie lasts 90
    // days, but an account can be deactivated. Whichever screen notices
    // it first, everyone lands on the login form.
    effect(() => {
      if (this.api.sessionExpired()) {
        this.api.acknowledgeSessionExpiry();
        void this.session.signOut();
        void this.router.navigate(['/login']);
      }
    });
  }
}
