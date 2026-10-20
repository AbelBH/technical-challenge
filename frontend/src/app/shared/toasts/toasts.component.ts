import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { NotificationService } from '../../core/notifications/notification.service';

@Component({
  selector: 'app-toasts',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="toasts" aria-live="polite">
      @for (n of notifications.notifications(); track n.id) {
        <div class="toast" [class]="'toast--' + n.type" role="status">
          <span>{{ n.message }}</span>
          <button type="button" aria-label="Cerrar" (click)="notifications.dismiss(n.id)">×</button>
        </div>
      }
    </div>
  `,
  styles: `
    .toasts {
      position: fixed;
      right: 1rem;
      bottom: 1rem;
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
      z-index: 1000;
      max-width: min(360px, calc(100vw - 2rem));
    }
    .toast {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 0.75rem;
      padding: 0.75rem 1rem;
      border-radius: var(--radius);
      color: #fff;
      box-shadow: var(--shadow);
      animation: slide-in 0.2s ease-out;
    }
    .toast--success { background: var(--success); }
    .toast--error { background: var(--danger); }
    button {
      background: none;
      border: none;
      color: inherit;
      font-size: 1.25rem;
      cursor: pointer;
      line-height: 1;
    }
    @keyframes slide-in {
      from { transform: translateY(0.5rem); opacity: 0; }
      to { transform: translateY(0); opacity: 1; }
    }
  `,
})
export class ToastsComponent {
  protected readonly notifications = inject(NotificationService);
}
