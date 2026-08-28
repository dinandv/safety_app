import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { OfflineError } from '../../core/api-client';
import { Session } from '../../core/session';
import { UiButton } from '../../ui/button';
import { UiIcon } from '../../ui/icon';
import { UiNote } from '../../ui/note';

/**
 * No passwords: an address, a six-digit code by e-mail, and then a
 * session that lasts long enough that most people never come back here.
 *
 * The response is identical whether the address is known or not. That is
 * the whole point of the wording below — "als dit adres bij ons bekend
 * is" is not vagueness, it is the feature: this form must not tell a
 * stranger who is a volunteer here.
 *
 * A magic link lands on this same page with the address and code in the
 * query string, so following it from a phone skips both steps.
 */
@Component({
  selector: 'app-login-page',
  imports: [FormsModule, UiButton, UiIcon, UiNote],
  template: `
    <main>
      <div class="stack">
        <header>
          <div class="eyebrow">Veiligheidsteam</div>
          <h1>Inloggen</h1>
        </header>

        @if (step() === 'email') {
          <p class="muted">
            Vul je e-mailadres in. Je krijgt een code van zes cijfers waarmee je binnenkomt — een
            wachtwoord heb je hier niet nodig.
          </p>

          <form (ngSubmit)="requestCode()">
            <label for="email">E-mailadres</label>
            <input
              id="email"
              name="email"
              type="email"
              autocomplete="email"
              inputmode="email"
              required
              [(ngModel)]="email"
              [disabled]="busy()"
            />
            <button
              uiButton
              variant="primary"
              size="lg"
              fullWidth
              type="submit"
              [disabled]="busy()"
            >
              <ui-icon name="mail" [size]="18" />
              {{ busy() ? 'Bezig…' : 'Stuur me een code' }}
            </button>
          </form>
        } @else {
          <p class="muted">
            Als dit adres bij ons bekend is, staat er nu een code van zes cijfers in de mail. Hij is
            een kwartier geldig en werkt één keer.
          </p>

          <form (ngSubmit)="confirm()">
            <label for="code">Code</label>
            <input
              id="code"
              name="code"
              type="text"
              inputmode="numeric"
              autocomplete="one-time-code"
              maxlength="6"
              class="code"
              required
              [(ngModel)]="code"
              [disabled]="busy()"
            />
            <button
              uiButton
              variant="primary"
              size="lg"
              fullWidth
              type="submit"
              [disabled]="busy()"
            >
              {{ busy() ? 'Bezig…' : 'Inloggen' }}
            </button>
          </form>

          <button uiButton variant="ghost" size="md" type="button" (click)="backToEmail()">
            Ander e-mailadres
          </button>
        }

        @if (message()) {
          <ui-note tone="gap" title="Inloggen lukte niet">{{ message() }}</ui-note>
        }
      </div>
    </main>
  `,
  styles: [
    `
      main {
        min-height: 100dvh;
        display: flex;
        align-items: center;
        justify-content: center;
        padding: var(--gutter-screen);
      }

      .stack {
        width: 100%;
        max-width: 380px;
      }

      h1 {
        font-size: var(--text-title-1);
        margin-top: 4px;
      }

      form {
        display: flex;
        flex-direction: column;
        gap: var(--space-2);
      }

      label {
        font-size: var(--text-label);
        color: var(--text-muted);
      }

      input {
        font-family: var(--font-ui);
        font-size: var(--text-body-lg);
        min-height: var(--hit-min);
        padding: 0 12px;
        border: 1px solid var(--border-strong);
        border-radius: var(--radius-sm);
        background: var(--surface-card);
        color: var(--text-body);
        margin-bottom: var(--space-2);
      }

      input.code {
        font-family: var(--font-data);
        letter-spacing: 0.35em;
        text-align: center;
      }

      input:focus-visible {
        outline: none;
        border-color: var(--border-focus);
        box-shadow: var(--focus-ring);
      }
    `,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginPage {
  private readonly session = inject(Session);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly step = signal<'email' | 'code'>('email');
  readonly busy = signal(false);
  readonly message = signal<string | null>(null);

  email = '';
  code = '';

  constructor() {
    const params = this.route.snapshot.queryParamMap;
    const email = params.get('email');
    const code = params.get('code');
    if (email && code) {
      this.email = email;
      this.code = code;
      this.step.set('code');
      void this.confirm();
    }
  }

  async requestCode(): Promise<void> {
    if (!this.email.trim() || this.busy()) return;
    this.busy.set(true);
    this.message.set(null);
    try {
      await this.session.requestCode(this.email.trim());
      this.step.set('code');
    } catch (cause) {
      this.message.set(describe(cause));
    } finally {
      this.busy.set(false);
    }
  }

  async confirm(): Promise<void> {
    if (!this.code.trim() || this.busy()) return;
    this.busy.set(true);
    this.message.set(null);
    try {
      await this.session.confirmCode(this.email.trim(), this.code.trim());
      await this.router.navigate(['/']);
    } catch (cause) {
      // One message for a wrong code, an expired code and an unknown
      // address alike — anything more specific tells a stranger which of
      // the three it was.
      this.message.set(describe(cause));
      this.code = '';
    } finally {
      this.busy.set(false);
    }
  }

  backToEmail(): void {
    this.step.set('email');
    this.code = '';
    this.message.set(null);
  }
}

function describe(cause: unknown): string {
  return cause instanceof OfflineError
    ? 'Geen verbinding. Probeer het opnieuw zodra je weer bereik hebt.'
    : 'Deze combinatie klopt niet, of de code is verlopen. Vraag een nieuwe code aan.';
}
