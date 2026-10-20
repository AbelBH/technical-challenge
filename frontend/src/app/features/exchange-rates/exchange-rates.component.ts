import { DatePipe, DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal } from '@angular/core';
import { finalize } from 'rxjs';
import { AuthService } from '../../core/auth/auth.service';
import { getErrorMessage } from '../../core/http/problem-details';
import {
  CreateExchangeRateRequest,
  ExchangeRate,
  UpdateExchangeRateRequest,
} from '../../core/models/exchange-rate.models';
import { NotificationService } from '../../core/notifications/notification.service';
import { ExchangeRateService } from '../../core/services/exchange-rate.service';
import { ExchangeRateFormComponent } from './exchange-rate-form/exchange-rate-form.component';

type FormState = { mode: 'closed' } | { mode: 'create' } | { mode: 'edit'; rate: ExchangeRate };

@Component({
  selector: 'app-exchange-rates',
  imports: [DecimalPipe, DatePipe, ExchangeRateFormComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './exchange-rates.component.html',
  styleUrl: './exchange-rates.component.scss',
})
export class ExchangeRatesComponent implements OnInit {
  private readonly service = inject(ExchangeRateService);
  private readonly notifications = inject(NotificationService);
  protected readonly auth = inject(AuthService);

  protected readonly rates = signal<ExchangeRate[]>([]);
  protected readonly loading = signal(true);
  protected readonly saving = signal(false);
  protected readonly loadError = signal<string | null>(null);
  protected readonly formState = signal<FormState>({ mode: 'closed' });
  protected readonly search = signal('');

  protected readonly filteredRates = computed(() => {
    const term = this.search().trim().toUpperCase();
    return term
      ? this.rates().filter((r) => r.sourceCurrency.includes(term) || r.targetCurrency.includes(term))
      : this.rates();
  });

  protected readonly editingRate = computed(() => {
    const state = this.formState();
    return state.mode === 'edit' ? state.rate : null;
  });

  ngOnInit(): void {
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.loadError.set(null);

    this.service
      .getAll()
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (rates) => this.rates.set(rates),
        error: (err) => this.loadError.set(getErrorMessage(err)),
      });
  }

  protected openCreate(): void {
    this.formState.set({ mode: 'create' });
  }

  protected openEdit(rate: ExchangeRate): void {
    this.formState.set({ mode: 'edit', rate });
  }

  protected closeForm(): void {
    this.formState.set({ mode: 'closed' });
  }

  protected onCreate(request: CreateExchangeRateRequest): void {
    this.saving.set(true);
    this.service
      .create(request)
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: (created) => {
          const pairs = created.map((r) => `${r.sourceCurrency} → ${r.targetCurrency}`).join(' y ');
          this.notifications.success(`Tipo de cambio creado: ${pairs}.`);
          this.closeForm();
          this.load();
        },
        error: (err) => this.notifications.error(getErrorMessage(err)),
      });
  }

  protected onUpdate({ id, request }: { id: number; request: UpdateExchangeRateRequest }): void {
    this.saving.set(true);
    this.service
      .update(id, request)
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: (updated) => {
          this.rates.update((list) => list.map((r) => (r.id === updated.id ? updated : r)));
          this.notifications.success(
            `Tipo de cambio ${updated.sourceCurrency} → ${updated.targetCurrency} actualizado.`,
          );
          this.closeForm();
        },
        error: (err) => this.notifications.error(getErrorMessage(err)),
      });
  }

  protected remove(rate: ExchangeRate): void {
    if (!confirm(`¿Eliminar el tipo de cambio ${rate.sourceCurrency} → ${rate.targetCurrency}?`)) {
      return;
    }

    this.service.delete(rate.id).subscribe({
      next: () => {
        this.rates.update((list) => list.filter((r) => r.id !== rate.id));
        this.notifications.success(`Tipo de cambio ${rate.sourceCurrency} → ${rate.targetCurrency} eliminado.`);
        if (this.editingRate()?.id === rate.id) this.closeForm();
      },
      error: (err) => this.notifications.error(getErrorMessage(err)),
    });
  }
}
