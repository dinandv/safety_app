import { ChangeDetectionStrategy, Component, Injectable, signal } from '@angular/core';

export type ToastTone = 'info' | 'success' | 'error';

export interface Toast {
  readonly id: number;
  readonly tone: ToastTone;
  readonly title: string;
  readonly description?: string;
}

/**
 * Confirmation for things that changed on the server. Taking an open
 * spot is the main one: the list under it updates as well, but a
 * volunteer standing in a hall wants to be told, not to have to compare
 * two screens.
 */
@Injectable({ providedIn: 'root' })
export class Toasts {
  private next = 1;
  private readonly items = signal<Toast[]>([]);

  readonly all = this.items.asReadonly();

  show(tone: ToastTone, title: string, description?: string): void {
    const toast: Toast = { id: this.next++, tone, title, description };
    this.items.update((list) => [...list, toast]);
    setTimeout(() => this.dismiss(toast.id), 6000);
  }

  success(title: string, description?: string): void {
    this.show('success', title, description);
  }

  error(title: string, description?: string): void {
    this.show('error', title, description);
  }

  dismiss(id: number): void {
    this.items.update((list) => list.filter((toast) => toast.id !== id));
  }
}

@Component({
  selector: 'ui-toast-host',
  template: `
    @for (toast of toasts.all(); track toast.id) {
      <div class="toast" role="status">
        <span class="dot" [class]="toast.tone"></span>
        <div class="text">
          <span class="title">{{ toast.title }}</span>
          @if (toast.description) {
            <span class="description">{{ toast.description }}</span>
          }
        </div>
        <button type="button" aria-label="Sluiten" (click)="toasts.dismiss(toast.id)">×</button>
      </div>
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
  styles: [
    `
      :host {
        position: fixed;
        left: 50%;
        transform: translateX(-50%);
        bottom: calc(var(--tab-bar-height, 64px) + var(--space-4));
        z-index: 20;
        display: flex;
        flex-direction: column;
        gap: var(--space-2);
        width: min(400px, calc(100vw - 2 * var(--gutter-screen)));
        pointer-events: none;
      }

      .toast {
        pointer-events: auto;
        display: flex;
        gap: var(--space-3);
        align-items: flex-start;
        background: var(--surface-inverse);
        color: var(--text-inverse);
        border-radius: var(--radius-md);
        box-shadow: var(--shadow-overlay);
        padding: var(--space-4);
      }

      .dot {
        width: 8px;
        height: 8px;
        border-radius: 50%;
        margin-top: 6px;
        flex: none;
      }
      .dot.info {
        background: var(--status-info);
      }
      .dot.success {
        background: var(--status-filled);
      }
      .dot.error {
        background: var(--status-gap);
      }

      .text {
        display: flex;
        flex-direction: column;
        gap: 2px;
        flex: 1;
        min-width: 0;
      }

      .title {
        font-size: var(--text-body-md);
        font-weight: var(--weight-medium);
      }

      .description {
        font-size: var(--text-body-sm);
        color: var(--warm-300);
      }

      button {
        border: none;
        background: transparent;
        color: var(--warm-300);
        cursor: pointer;
        font-size: 16px;
        line-height: 1;
        min-height: var(--hit-min);
        min-width: 32px;
      }
    `,
  ],
})
export class UiToastHost {
  constructor(readonly toasts: Toasts) {}
}
