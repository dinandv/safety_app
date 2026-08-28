import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { UiToastHost } from './ui/toasts';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, UiToastHost],
  template: `
    <router-outlet />
    <ui-toast-host />
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class App {}
