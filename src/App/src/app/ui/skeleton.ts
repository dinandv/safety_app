import { ChangeDetectionStrategy, Component, input } from '@angular/core';

/**
 * The loading state shows the shape of what is coming, not a spinner:
 * the overview arrives in one call, so the layout never jumps once it
 * does.
 */
@Component({
  selector: 'ui-skeleton',
  template: '',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    'aria-hidden': 'true',
    '[style.width]': 'width()',
    '[style.height]': 'height()',
    '[style.border-radius]': 'radius()',
  },
  styles: [
    `
      :host {
        display: block;
        background: var(--warm-200);
        animation: pulse 1.4s var(--ease-standard) infinite;
      }

      @keyframes pulse {
        0%,
        100% {
          opacity: 0.55;
        }
        50% {
          opacity: 0.25;
        }
      }

      @media (prefers-reduced-motion: reduce) {
        :host {
          animation: none;
          opacity: 0.45;
        }
      }
    `,
  ],
})
export class UiSkeleton {
  readonly width = input<string>('100%');
  readonly height = input<string>('16px');
  readonly radius = input<string>('var(--radius-xs)');
}
