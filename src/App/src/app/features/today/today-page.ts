import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ApiClient, ApiError, OfflineError } from '../../core/api-client';
import { CachedResource } from '../../core/cached-resource';
import { LongDatePipe, TimeRangePipe, formatTime } from '../../core/dutch-date';
import type {
  OpenSpots,
  PhoneVisibilityState,
  RoleGroup,
  TodayResponse,
} from '../../core/api.types';
import { Toasts } from '../../ui/toasts';
import { UiBadge } from '../../ui/badge';
import { UiButton } from '../../ui/button';
import { UiIcon } from '../../ui/icon';
import { UiNote } from '../../ui/note';
import { UiSkeleton } from '../../ui/skeleton';
import { UiVestChip, vestColor } from '../../ui/vest-chip';

/**
 * Who is on duty today, per team role.
 *
 * This is the first tab and the reason the application exists: the
 * loudest complaint from practice is that nobody knows who is on today,
 * and the person asking usually has no shift themselves. So the screen
 * is written for them first — a full roster, not a personal agenda — and
 * an unfilled spot is a row of its own rather than a row that is simply
 * missing.
 */
@Component({
  selector: 'app-today-page',
  imports: [LongDatePipe, TimeRangePipe, UiBadge, UiButton, UiIcon, UiNote, UiSkeleton, UiVestChip],
  templateUrl: './today-page.html',
  styleUrl: './today-page.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '(window:online)': 'refresh()',
    '(document:visibilitychange)': 'refreshWhenVisible()',
  },
})
export class TodayPage {
  private readonly api = inject(ApiClient);
  private readonly toasts = inject(Toasts);

  readonly resource = new CachedResource<TodayResponse>(this.api, '/api/today');
  readonly claiming = signal<string | null>(null);

  constructor() {
    void this.resource.reload();
  }

  refresh(): void {
    void this.resource.reload();
  }

  refreshWhenVisible(): void {
    // Coming back to the app after an hour in a pocket should not show
    // an overview from an hour ago.
    if (document.visibilityState === 'visible') this.refresh();
  }

  vestColor = vestColor;

  occupancyLabel(filled: number, required: number): string {
    return `${filled} van ${required} plekken bezet`;
  }

  groupLabel(group: RoleGroup): string {
    return `${group.people.length}/${group.requiredCount} bezet`;
  }

  /** "Nog niemand", "Nog één plek open", "Nog 2 plekken open". */
  gapTitle(group: RoleGroup, spots: OpenSpots): string {
    if (group.people.length === 0) return 'Nog niemand';
    return spots.count === 1 ? 'Nog één plek open' : `Nog ${spots.count} plekken open`;
  }

  gapReason(spots: OpenSpots): string {
    return spots.reason === 'Withdrawn' && spots.withdrawnByFirstName
      ? `${spots.withdrawnByFirstName} heeft zich afgemeld`
      : 'Nog niet ingevuld bij het plannen';
  }

  /** "09:11", or an empty string while nothing has been fetched yet. */
  fetchedAtTime(): string {
    const at = this.resource.fetchedAt();
    return at ? formatTime(at) : '';
  }

  /** Never show a cached screen without saying how old it is. */
  updatedAt(): string {
    const at = this.fetchedAtTime();
    return at ? `bijgewerkt om ${at}` : '';
  }

  /**
   * The reason a number is missing comes from the server, because "you
   * are not on this duty" and "the duty has not started yet" are
   * different sentences and guessing between them makes the rule look
   * arbitrary.
   */
  hiddenPhoneLabel(state: PhoneVisibilityState): string {
    return state === 'NotScheduled'
      ? 'Nummer alleen voor dienstgenoten'
      : 'Nummer zichtbaar tijdens de dienst';
  }

  phoneFootnote(state: PhoneVisibilityState): string {
    switch (state) {
      case 'Visible':
        return 'Nummers zijn zichtbaar zolang deze dienst loopt, en alleen voor wie vandaag ingedeeld staat.';
      case 'OutsideShiftWindow':
        return 'Je staat vandaag ingedeeld. De nummers van je dienstgenoten verschijnen kort voordat de dienst begint.';
      default:
        return 'Telefoonnummers van dienstgenoten zie je alleen als je zelf op deze dienst staat.';
    }
  }

  telHref(phone: string): string {
    return `tel:${phone.replace(/\s/g, '')}`;
  }

  async claim(shiftId: string): Promise<void> {
    if (this.claiming()) return;
    this.claiming.set(shiftId);
    try {
      await this.api.post(`/api/shifts/${shiftId}/claim`);
      await this.resource.reload();
      this.toasts.success(
        'Je staat op de dienst',
        'Het dagoverzicht is bijgewerkt en de planner krijgt een bericht.',
      );
    } catch (cause) {
      this.toasts.error(...describeClaimFailure(cause));
      // Someone else may have taken it a second earlier, so the list is
      // refreshed either way rather than left showing a spot that is gone.
      await this.resource.reload();
    } finally {
      this.claiming.set(null);
    }
  }
}

function describeClaimFailure(cause: unknown): [string, string] {
  if (cause instanceof OfflineError) {
    return ['Geen verbinding', 'Probeer het opnieuw zodra je weer bereik hebt.'];
  }
  if (cause instanceof ApiError) {
    switch (cause.reason) {
      case 'already_taken':
        return ['Net te laat', 'Iemand anders heeft deze plek zojuist genomen.'];
      case 'already_assigned':
        return ['Je stond er al op', 'Deze dienst staat al op jouw naam.'];
      case 'not_qualified':
        return [
          'Deze rol kan nog niet',
          'Je certificaat voor deze rol ontbreekt of is verlopen. Vraag het de planner.',
        ];
      case 'shift_started':
        return ['Deze dienst is al begonnen', 'Bel het hoofd-BHV als je alsnog kunt komen.'];
      default:
        return ['Dat lukte niet', 'Probeer het zo nog eens.'];
    }
  }
  return ['Dat lukte niet', 'Probeer het zo nog eens.'];
}
