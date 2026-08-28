import { ChangeDetectionStrategy, Component, booleanAttribute, input } from '@angular/core';

export type ButtonVariant = 'primary' | 'accent' | 'secondary' | 'ghost' | 'danger';
export type ButtonSize = 'sm' | 'md' | 'lg';

/**
 * Applied to a real `button` or `a`, so keyboard behaviour, `disabled`
 * and `href` keep working without being reimplemented.
 *
 * `lg` is exactly the minimum touch target. Everything a volunteer taps
 * during an event — calling a colleague, taking an open spot — uses it.
 */
@Component({
  selector: 'button[uiButton], a[uiButton]',
  template: '<ng-content />',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[class]': '"ui-button v-" + variant() + " s-" + size()',
    '[class.full]': 'fullWidth()',
  },
  styles: [
    `
      :host {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        gap: var(--space-2);
        font-family: var(--font-ui);
        font-weight: var(--weight-medium);
        border-radius: var(--radius-sm);
        border: 1px solid transparent;
        cursor: pointer;
        text-decoration: none;
        white-space: nowrap;
        line-height: 1;
        transition:
          background var(--duration-fast) var(--ease-standard),
          color var(--duration-fast) var(--ease-standard),
          border-color var(--duration-fast) var(--ease-standard);
      }

      :host(.full) {
        width: 100%;
      }

      :host(:active:not(:disabled)) {
        transform: scale(var(--press-scale));
      }

      :host(.s-sm) {
        font-size: var(--text-body-sm);
        padding: 0 12px;
        height: 32px;
      }
      :host(.s-md) {
        font-size: var(--text-body-md);
        padding: 0 16px;
        height: 40px;
      }
      :host(.s-lg) {
        font-size: var(--text-body-lg);
        padding: 0 22px;
        height: var(--hit-min);
      }

      :host(.v-primary) {
        background: var(--action-primary);
        color: var(--text-inverse);
      }
      :host(.v-primary:hover:not(:disabled)) {
        background: var(--action-primary-hover);
      }

      :host(.v-accent) {
        background: var(--action-accent);
        color: var(--warm-900);
      }
      :host(.v-accent:hover:not(:disabled)) {
        background: var(--action-accent-hover);
      }

      :host(.v-secondary) {
        background: var(--surface-card);
        color: var(--text-heading);
        border-color: var(--border-strong);
      }
      :host(.v-secondary:hover:not(:disabled)) {
        background: var(--surface-hover);
      }

      :host(.v-ghost) {
        background: transparent;
        color: var(--text-heading);
      }
      :host(.v-ghost:hover:not(:disabled)) {
        background: var(--surface-hover);
      }

      :host(.v-danger) {
        background: var(--red-500);
        color: var(--text-inverse);
      }
      :host(.v-danger:hover:not(:disabled)) {
        background: var(--red-700);
      }

      :host(:disabled),
      :host(.v-primary:disabled),
      :host(.v-accent:disabled) {
        background: var(--action-disabled);
        color: var(--text-faint);
        border-color: transparent;
        cursor: not-allowed;
        transform: none;
      }
    `,
  ],
})
export class UiButton {
  readonly variant = input<ButtonVariant>('primary');
  readonly size = input<ButtonSize>('md');
  readonly fullWidth = input(false, { transform: booleanAttribute });
}
