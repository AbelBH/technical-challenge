import { DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { getErrorMessage } from '../../core/http/problem-details';
import { ConversionResponse } from '../../core/models/conversion.models';
import { ExchangeRate } from '../../core/models/exchange-rate.models';
import { ConversionService } from '../../core/services/conversion.service';
import { ExchangeRateService } from '../../core/services/exchange-rate.service';

@Component({
  selector: 'app-converter',
  imports: [ReactiveFormsModule, DecimalPipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './converter.component.html',
  styleUrl: './converter.component.scss',
})
export class ConverterComponent implements OnInit {
  private readonly conversionService = inject(ConversionService);
  private readonly exchangeRateService = inject(ExchangeRateService);

  private readonly rates = signal<ExchangeRate[]>([]);

  /** Monedas disponibles, obtenidas de los tipos de cambio configurados. */
  protected readonly currencies = computed(() =>
    [...new Set(this.rates().flatMap((r) => [r.sourceCurrency, r.targetCurrency]))].sort(),
  );

  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly result = signal<ConversionResponse | null>(null);

  protected readonly form = inject(FormBuilder).nonNullable.group({
    amount: [100, [Validators.required, Validators.min(0)]],
    sourceCurrency: ['USD', Validators.required],
    targetCurrency: ['PEN', Validators.required],
  });

  ngOnInit(): void {
    this.exchangeRateService.getAll().subscribe({
      next: (rates) => this.rates.set(rates),
      error: (err) => this.error.set(getErrorMessage(err)),
    });
  }

  protected swap(): void {
    const { sourceCurrency, targetCurrency } = this.form.getRawValue();
    this.form.patchValue({ sourceCurrency: targetCurrency, targetCurrency: sourceCurrency });
    this.result.set(null);
  }

  protected convert(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    this.conversionService
      .convert(this.form.getRawValue())
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (response) => this.result.set(response),
        error: (err) => {
          this.result.set(null);
          this.error.set(getErrorMessage(err));
        },
      });
  }
}
