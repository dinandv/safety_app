import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { vestColor } from './vest-chip';

/**
 * The standard surface. `markerColor` paints the left edge — used to
 * carry a vest colour down the side of a shift, so a card is
 * recognisable before a word of it is read.
 */
@Component({
  selector: 'ui-card',
  template: `
    @if (title()) {
      <header>
        <div class="titles">
          <h3>{{ title() }}</h3>
          @if (meta()) {
            <span class="meta">{{ meta() }}</span>
          }
        </div>
        <ng-content select="[cardActions]" />
      </header>
    }
    <div class="body" [class.padded-top]="!title()">
      <ng-content />
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { '[style.border-left]': 'marker()' },
  styles: [
    `
      :host {
        display: block;
        background: var(--surface-card);
        border: 1px solid var(--border-hairline);
        border-radius: var(--radius-md);
        box-shadow: var(--shadow-card);
        font-family: var(--font-ui);
        overflow: hidden;
      }

      header {
        display: flex;
        align-items: baseline;
        justify-content: space-between;
        gap: var(--space-4);
        padding: var(--gutter-card) var(--gutter-card) var(--space-3);
      }

      .titles {
        display: flex;
        flex-direction: column;
        gap: 2px;
        min-width: 0;
      }

      h3 {
        font-size: var(--text-title-3);
      }

      .meta {
        font-size: var(--text-body-sm);
        color: var(--text-muted);
      }

      .body {
        padding: 0 var(--gutter-card) var(--gutter-card);
      }

      .body.padded-top {
        padding-top: var(--gutter-card);
      }
    `,
  ],
})
export class UiCard {
  readonly title = input<string | null>(null);
  readonly meta = input<string | null>(null);
  readonly markerColor = input<string | null>(null);

  readonly marker = computed(() => {
    const color = this.markerColor();
    return color ? `var(--border-width-marker) solid ${vestColor(color)}` : null;
  });
}
