import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

/**
 * Named vest colours map to tokens; anything else is passed through as a
 * literal CSS colour. `team_role.vest_color` is free text, and a tenant
 * that fills in a hex code should still get the colour it asked for
 * rather than a silent grey.
 */
const VEST_TOKENS: Record<string, string> = {
  red: 'var(--vest-red)',
  orange: 'var(--vest-orange)',
  yellow: 'var(--vest-yellow)',
  green: 'var(--vest-green)',
  blue: 'var(--vest-blue)',
  white: 'var(--vest-white)',
};

export function vestColor(value: string | null | undefined): string {
  if (!value) return 'var(--warm-300)';
  return VEST_TOKENS[value.trim().toLowerCase()] ?? value;
}

/**
 * The team role with its vest colour. On site people recognise each
 * other by the vest, not by a job title, so the screen shows the same
 * thing they are looking at.
 */
@Component({
  selector: 'ui-vest-chip',
  template: `
    <span class="swatch" [style.background]="color()"></span>
    <span>{{ roleName() }}</span>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { '[class.sm]': "size() === 'sm'" },
  styles: [
    `
      :host {
        display: inline-flex;
        align-items: center;
        gap: 7px;
        font-family: var(--font-ui);
        font-size: var(--text-label);
        font-weight: var(--weight-medium);
        color: var(--text-body);
        background: var(--surface-card);
        border: 1px solid var(--border-hairline);
        border-radius: var(--radius-pill);
        padding: 3px 10px 3px 5px;
      }

      :host(.sm) {
        font-size: var(--text-micro);
        padding: 2px 8px 2px 4px;
      }

      .swatch {
        width: 14px;
        height: 14px;
        border-radius: 50%;
        border: 1px solid rgba(26, 23, 20, 0.15);
        flex: none;
      }

      :host(.sm) .swatch {
        width: 12px;
        height: 12px;
      }
    `,
  ],
})
export class UiVestChip {
  readonly roleName = input.required<string>();
  readonly vestColor = input<string | null>(null);
  readonly size = input<'sm' | 'md'>('md');

  readonly color = computed(() => vestColor(this.vestColor()));
}
