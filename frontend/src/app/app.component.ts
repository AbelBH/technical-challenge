import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ToastsComponent } from './shared/toasts/toasts.component';

@Component({
  imports: [RouterOutlet, ToastsComponent],
  selector: 'app-root',
  template: `
    <router-outlet />
    <app-toasts />
  `,
})
export class App {}
