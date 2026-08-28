import { ChangeDetectionStrategy, Component, booleanAttribute, input } from '@angular/core';

export type BadgeTone = 'neutral' | 'filled' | 'gap' | 'warning' | 'info' | 'accent';

/** A short status, never a sentence. "2/3 bezet", "Open oproep". */
@Component({
  selector: 'ui-badge',
  template: `
    @if (dot()) {
      <span class="dot"></span>
    }
    <ng-content />
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { '[class]': '"t-" + tone()' },
  styles: [
    `
      :host {
        display: inline-flex;
        align-items: center;
        gap: 6px;
        font-family: var(--font-ui);
        font-size: var(--text-label);
        font-weight: var(--weight-medium);
        padding: 3px 9px;
        border-radius: var(--radius-pill);
        line-height: 1.4;
        white-space: nowrap;
      }

      .dot {
        width: 6px;
        height: 6px;
        border-radius: 50%;
        background: currentColor;
      }

      :host(.t-neutral) {
        background: var(--warm-100);
        color: var(--warm-700);
      }
      :host(.t-filled) {
        background: var(--green-50);
        color: var(--green-700);
      }
      :host(.t-gap) {
        background: var(--red-50);
        color: var(--red-700);
      }
      :host(.t-warning) {
        background: var(--amber-50);
        color: var(--amber-700);
      }
      :host(.t-info) {
        background: var(--blue-50);
        color: var(--blue-700);
      }
      :host(.t-accent) {
        background: var(--signal-orange-50);
        color: var(--signal-orange-700);
      }
    `,
  ],
})
export class UiBadge {
  readonly tone = input<BadgeTone>('neutral');
  readonly dot = input(false, { transform: booleanAttribute });
}
