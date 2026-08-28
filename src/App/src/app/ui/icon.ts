import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  effect,
  inject,
  input,
} from '@angular/core';
import {
  BookOpen,
  CalendarCheck,
  CalendarDays,
  Check,
  ChevronRight,
  CircleAlert,
  Hand,
  Info,
  LogOut,
  Mail,
  Megaphone,
  Phone,
  QrCode,
  RefreshCw,
  User,
  WifiOff,
  createElement,
  type IconNode,
} from 'lucide';

/**
 * Icons are bundled, not pulled from a CDN: this app has to draw itself
 * with no connection at all. Only the glyphs actually used are imported,
 * so the bundle carries a dozen paths rather than a whole icon set.
 */
const ICONS = {
  'book-open': BookOpen,
  'calendar-check': CalendarCheck,
  'calendar-days': CalendarDays,
  check: Check,
  'chevron-right': ChevronRight,
  'circle-alert': CircleAlert,
  hand: Hand,
  info: Info,
  'log-out': LogOut,
  mail: Mail,
  megaphone: Megaphone,
  phone: Phone,
  'qr-code': QrCode,
  'refresh-cw': RefreshCw,
  user: User,
  'wifi-off': WifiOff,
} satisfies Record<string, IconNode>;

export type IconName = keyof typeof ICONS;

@Component({
  selector: 'ui-icon',
  template: '',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { 'aria-hidden': 'true' },
  styles: [
    `
      :host {
        display: inline-flex;
        flex: none;
        line-height: 0;
      }
      :host ::ng-deep svg {
        width: var(--icon-size, 16px);
        height: var(--icon-size, 16px);
      }
    `,
  ],
})
export class UiIcon {
  readonly name = input.required<IconName>();
  readonly size = input<number>(16);

  private readonly host = inject(ElementRef<HTMLElement>);

  constructor() {
    effect(() => {
      const element = this.host.nativeElement as HTMLElement;
      element.style.setProperty('--icon-size', `${this.size()}px`);
      element.replaceChildren(createElement(ICONS[this.name()], { 'stroke-width': '1.75' }));
    });
  }
}
