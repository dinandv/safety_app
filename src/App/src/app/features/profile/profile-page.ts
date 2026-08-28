import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Session } from '../../core/session';
import { UiButton } from '../../ui/button';
import { UiCard } from '../../ui/card';
import { UiIcon } from '../../ui/icon';

const ROLE_LABELS: Record<string, string> = {
  PlatformAdmin: 'Platformbeheerder',
  TenantAdmin: 'Beheerder',
  Planner: 'Planner',
  Participant: 'Deelnemer',
};

/** Who you are signed in as, and the way out. */
@Component({
  selector: 'app-profile-page',
  imports: [UiButton, UiCard, UiIcon],
  template: `
    @let user = session.user();

    <div class="stack">
      <h2>Ik</h2>

      <ui-card [title]="user?.displayName ?? 'Onbekend'" [meta]="roles()">
        <p class="muted small">
          Je blijft ingelogd op dit toestel. Log je uit, dan verdwijnt ook de offline kopie van het
          dagoverzicht en de contactkaart.
        </p>
      </ui-card>

      <div>
        <button uiButton variant="secondary" size="md" type="button" (click)="signOut()">
          <ui-icon name="log-out" />
          Uitloggen
        </button>
      </div>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProfilePage {
  private readonly router = inject(Router);
  readonly session = inject(Session);

  roles(): string {
    const roles = this.session.user()?.roles ?? [];
    if (roles.length === 0) return 'Deelnemer';
    return roles.map((role) => ROLE_LABELS[role] ?? role).join(' · ');
  }

  async signOut(): Promise<void> {
    await this.session.signOut();
    await this.router.navigate(['/login']);
  }
}
