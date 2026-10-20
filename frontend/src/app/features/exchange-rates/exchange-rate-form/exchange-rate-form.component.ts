import { ChangeDetectionStrategy, Component, computed, effect, inject, input, output } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import {
  CreateExchangeRateRequest,
  ExchangeRate,
  UpdateExchangeRateRequest,
} from '../../../core/models/exchange-rate.models';

const CURRENCY_PATTERN = /^[A-Za-z]{3}$/;

/**
 * Formulario de creación/edición. Es un componente "presentacional":
 * no llama a la API, solo emite eventos y el componente padre decide qué hacer.
 */
@Component({
  selector: 'app-exchange-rate-form',
  imports: [ReactiveFormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './exchange-rate-form.component.html',
  styleUrl: './exchange-rate-form.component.scss',
})
export class ExchangeRateFormComponent {
  /** null = crear; con valor = editar ese tipo de cambio. */
  readonly rate = input<ExchangeRate | null>(null);
  readonly saving = input(false);

  readonly create = output<CreateExchangeRateRequest>();
  readonly update = output<{ id: number; request: UpdateExchangeRateRequest }>();
  readonly cancel = output<void>();

  protected readonly isEdit = computed(() => this.rate() !== null);

  protected readonly form = inject(FormBuilder).nonNullable.group({
    sourceCurrency: ['', [Validators.required, Validators.pattern(CURRENCY_PATTERN)]],
    targetCurrency: ['', [Validators.required, Validators.pattern(CURRENCY_PATTERN)]],
    rate: [0, [Validators.required, Validators.min(0.000001), Validators.max(1_000_000)]],
    createInverse: [true],
  });

  constructor() {
    // Cada vez que cambia el tipo de cambio a editar, se reinicia el formulario.
    effect(() => {
      const rate = this.rate();
      this.form.reset({
        sourceCurrency: rate?.sourceCurrency ?? '',
        targetCurrency: rate?.targetCurrency ?? '',
        rate: rate?.rate ?? 0,
        createInverse: true,
      });

      // En edición solo se puede cambiar la tasa (el par de monedas es la identidad del registro).
      if (rate) {
        this.form.controls.sourceCurrency.disable();
        this.form.controls.targetCurrency.disable();
      } else {
        this.form.controls.sourceCurrency.enable();
        this.form.controls.targetCurrency.enable();
      }
    });
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const current = this.rate();

    if (current) {
      this.update.emit({ id: current.id, request: { rate: value.rate } });
    } else {
      this.create.emit({
        sourceCurrency: value.sourceCurrency.toUpperCase(),
        targetCurrency: value.targetCurrency.toUpperCase(),
        rate: value.rate,
        createInverse: value.createInverse,
      });
    }
  }

  protected hasError(control: 'sourceCurrency' | 'targetCurrency' | 'rate'): boolean {
    const c = this.form.controls[control];
    return c.touched && c.invalid;
  }
}
