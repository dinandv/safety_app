import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { UiIcon, type IconName } from './icon';

export type NoteTone = 'warning' | 'info' | 'gap' | 'filled';

const ICONS: Record<NoteTone, IconName> = {
  warning: 'circle-alert',
  info: 'info',
  gap: 'circle-alert',
  filled: 'check',
};

/**
 * A signal with a reason. Used for the advisories of the day and for
 * telling someone their overview is a cached copy.
 *
 * Tone carries the meaning, so the text does not have to shout — the
 * house style has no exclamation marks.
 */
@Component({
  selector: 'ui-note',
  imports: [UiIcon],
  template: `
    <ui-icon class="glyph" [name]="icon()" [size]="16" />
    <div class="body">
      @if (title()) {
        <strong>{{ title() }}</strong>
      }
      <div class="text"><ng-content /></div>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { role: 'note', '[class]': '"t-" + tone()' },
  styles: [
    `
      :host {
        display: flex;
        gap: var(--space-3);
        align-items: flex-start;
        border-radius: var(--radius-md);
        border: 1px solid;
        padding: var(--space-4);
        font-family: var(--font-ui);
        color: var(--text-body);
      }

      .body {
        display: flex;
        flex-direction: column;
        gap: 4px;
        flex: 1;
        min-width: 0;
      }

      strong {
        font-size: var(--text-body-md);
        font-weight: var(--weight-semibold);
      }

      .text {
        font-size: var(--text-body-sm);
        line-height: var(--leading-normal);
      }

      .glyph {
        margin-top: 1px;
      }

      :host(.t-warning) {
        background: var(--amber-50);
        border-color: var(--amber-200);
      }
      :host(.t-warning) strong,
      :host(.t-warning) .glyph {
        color: var(--amber-700);
      }

      :host(.t-info) {
        background: var(--blue-50);
        border-color: var(--blue-200);
      }
      :host(.t-info) strong,
      :host(.t-info) .glyph {
        color: var(--blue-700);
      }

      :host(.t-gap) {
        background: var(--red-50);
        border-color: var(--red-200);
      }
      :host(.t-gap) strong,
      :host(.t-gap) .glyph {
        color: var(--red-700);
      }

      :host(.t-filled) {
        background: var(--green-50);
        border-color: var(--green-200);
      }
      :host(.t-filled) strong,
      :host(.t-filled) .glyph {
        color: var(--green-700);
      }
    `,
  ],
})
export class UiNote {
  readonly tone = input<NoteTone>('warning');
  readonly title = input<string | null>(null);

  readonly icon = computed<IconName>(() => ICONS[this.tone()]);
}
