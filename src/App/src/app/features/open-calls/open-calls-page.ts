import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ApiClient, ApiError, OfflineError } from '../../core/api-client';
import { CachedResource } from '../../core/cached-resource';
import { LongDatePipe, TimeRangePipe } from '../../core/dutch-date';
import type { OpenCall } from '../../core/api.types';
import { Toasts } from '../../ui/toasts';
import { UiBadge } from '../../ui/badge';
import { UiButton } from '../../ui/button';
import { UiIcon } from '../../ui/icon';
import { UiNote } from '../../ui/note';
import { UiSkeleton } from '../../ui/skeleton';
import { UiVestChip } from '../../ui/vest-chip';

/**
 * Spots that are still short of someone, filtered to the roles you are
 * actually qualified for — offering a shift that would be refused on
 * submit is worse than not offering it.
 *
 * Whoever claims first gets it, which is why the failure after losing a
 * race is worded as a plain fact rather than an error.
 */
@Component({
  selector: 'app-open-calls-page',
  imports: [LongDatePipe, TimeRangePipe, UiBadge, UiButton, UiIcon, UiNote, UiSkeleton, UiVestChip],
  template: `
    @let calls = resource.value();

    <div class="stack">
      <h2>Open oproepen</h2>

      @if (resource.isStale()) {
        <ui-note tone="warning" title="Je bent offline">
          Dit is de opgeslagen lijst. Reageren lukt pas weer met verbinding.
        </ui-note>
      }

      @if (!calls) {
        @if (resource.status() === 'error') {
          <p class="muted">We konden de open oproepen niet ophalen.</p>
        } @else {
          @for (row of [1, 2]; track row) {
            <ui-skeleton height="140px" radius="var(--radius-md)" />
          }
        }
      } @else if (calls.length === 0) {
        <p class="muted">
          Er staat op dit moment niets open voor jouw rollen. Dat is de normale situatie.
        </p>
      } @else {
        @for (call of calls; track call.shiftId) {
          <article class="call">
            <div class="row">
              <ui-vest-chip [roleName]="call.teamRoleName" [vestColor]="call.vestColor" />
              <ui-badge tone="accent">Open oproep</ui-badge>
            </div>
            <div>
              <div class="title">{{ call.start | longDate }}</div>
              <div class="data muted small">
                {{ call.start | timeRange: call.end }} · {{ call.locationName }}
              </div>
              <div class="muted small">{{ call.eventTitle }}</div>
            </div>
            <p class="muted small">{{ reason(call) }}. Wie het eerst reageert, krijgt de dienst.</p>
            @if (call.alreadyOnThisShift) {
              <p class="small muted">Je staat zelf al op deze dienst.</p>
            } @else {
              <button
                uiButton
                variant="accent"
                size="lg"
                type="button"
                fullWidth
                [disabled]="claiming() !== null"
                (click)="claim(call.shiftId)"
              >
                <ui-icon name="hand" [size]="18" />
                {{ claiming() === call.shiftId ? 'Bezig…' : 'Ik doe het' }}
              </button>
            }
          </article>
        }
      }
    </div>
  `,
  styles: [
    `
      .call {
        background: var(--surface-accent);
        border: 1px solid var(--signal-orange-200);
        border-radius: var(--radius-md);
        padding: var(--gutter-card);
        display: flex;
        flex-direction: column;
        gap: 10px;
      }

      .row {
        display: flex;
        justify-content: space-between;
        align-items: center;
        gap: 10px;
      }

      .title {
        font-size: var(--text-body-md);
        color: var(--text-heading);
      }
    `,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OpenCallsPage {
  private readonly api = inject(ApiClient);
  private readonly toasts = inject(Toasts);

  readonly resource = new CachedResource<OpenCall[]>(this.api, '/api/open-calls');
  readonly claiming = signal<string | null>(null);

  constructor() {
    void this.resource.reload();
  }

  reason(call: OpenCall): string {
    return call.reason === 'Withdrawn' && call.withdrawnByFirstName
      ? `${call.withdrawnByFirstName} heeft zich afgemeld`
      : 'Nog niemand ingedeeld';
  }

  async claim(shiftId: string): Promise<void> {
    if (this.claiming()) return;
    this.claiming.set(shiftId);
    try {
      await this.api.post(`/api/shifts/${shiftId}/claim`);
      this.toasts.success('Je staat op de dienst', 'Je vindt hem terug bij Mijn diensten.');
    } catch (cause) {
      if (cause instanceof OfflineError) {
        this.toasts.error('Geen verbinding', 'Probeer het opnieuw zodra je weer bereik hebt.');
      } else if (cause instanceof ApiError && cause.reason === 'already_taken') {
        this.toasts.error('Net te laat', 'Iemand anders was je voor.');
      } else {
        this.toasts.error('Dat lukte niet', 'Probeer het zo nog eens.');
      }
    } finally {
      this.claiming.set(null);
      await this.resource.reload();
    }
  }
}
