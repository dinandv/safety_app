import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ApiClient } from '../../core/api-client';
import { CachedResource } from '../../core/cached-resource';
import { LongDatePipe, TimeRangePipe } from '../../core/dutch-date';
import type { MyShift } from '../../core/api.types';
import { UiBadge } from '../../ui/badge';
import { UiIcon } from '../../ui/icon';
import { UiNote } from '../../ui/note';
import { UiSkeleton } from '../../ui/skeleton';
import { UiVestChip, vestColor } from '../../ui/vest-chip';

/** The shifts you are down for, nearest first. */
@Component({
  selector: 'app-my-shifts-page',
  imports: [LongDatePipe, TimeRangePipe, UiBadge, UiIcon, UiNote, UiSkeleton, UiVestChip],
  template: `
    @let shifts = resource.value();

    <div class="stack">
      <h2>Mijn diensten</h2>

      @if (resource.isStale()) {
        <ui-note tone="warning" title="Je bent offline">
          Dit is de opgeslagen versie. Wijzigingen van daarna staan er niet in.
        </ui-note>
      }

      @if (!shifts) {
        @if (resource.status() === 'error') {
          <p class="muted">We konden je diensten niet ophalen.</p>
        } @else {
          @for (row of [1, 2]; track row) {
            <ui-skeleton height="96px" radius="var(--radius-md)" />
          }
        }
      } @else if (shifts.length === 0) {
        <p class="muted">
          Je staat nu voor geen enkele dienst ingedeeld. Bij <strong>Oproepen</strong> zie je welke
          plekken nog open staan.
        </p>
      } @else {
        @for (shift of shifts; track shift.assignmentId) {
          <article class="card" [style.border-left-color]="vestColor(shift.vestColor)">
            <div class="row">
              <ui-vest-chip [roleName]="shift.teamRoleName" [vestColor]="shift.vestColor" />
              <ui-badge [tone]="shift.filledCount < shift.requiredCount ? 'gap' : 'filled'" dot>
                {{ shift.filledCount }}/{{ shift.requiredCount }} bezet
              </ui-badge>
            </div>
            <div>
              <div class="title">{{ shift.start | longDate }}</div>
              <div class="data muted small">
                {{ shift.start | timeRange: shift.end }} · {{ shift.locationName }}
              </div>
              <div class="muted small">{{ shift.eventTitle }}</div>
            </div>
            @if (shift.filledCount < shift.requiredCount) {
              <div class="hint small muted">
                <ui-icon name="megaphone" [size]="14" />
                Er staat nog een plek open op deze dienst.
              </div>
            }
          </article>
        }
      }
    </div>
  `,
  styles: [
    `
      .card {
        background: var(--surface-card);
        border: 1px solid var(--border-hairline);
        border-left: var(--border-width-marker) solid var(--warm-300);
        border-radius: var(--radius-md);
        padding: var(--gutter-card);
        display: flex;
        flex-direction: column;
        gap: 10px;
        box-shadow: var(--shadow-card);
      }

      .row {
        display: flex;
        justify-content: space-between;
        align-items: center;
        gap: 10px;
      }

      .title {
        font-family: var(--font-display);
        font-size: var(--text-title-3);
        font-weight: var(--weight-semibold);
        color: var(--text-heading);
      }

      .hint {
        display: flex;
        align-items: center;
        gap: 6px;
      }
    `,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MyShiftsPage {
  private readonly api = inject(ApiClient);

  readonly resource = new CachedResource<MyShift[]>(this.api, '/api/my/shifts');
  readonly vestColor = vestColor;

  constructor() {
    void this.resource.reload();
  }
}
