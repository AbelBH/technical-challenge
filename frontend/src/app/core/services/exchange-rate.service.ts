import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CreateExchangeRateRequest,
  ExchangeRate,
  UpdateExchangeRateRequest,
} from '../models/exchange-rate.models';

@Injectable({ providedIn: 'root' })
export class ExchangeRateService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/exchange-rates`;

  getAll(): Observable<ExchangeRate[]> {
    return this.http.get<ExchangeRate[]>(this.baseUrl);
  }

  create(request: CreateExchangeRateRequest): Observable<ExchangeRate[]> {
    return this.http.post<ExchangeRate[]>(this.baseUrl, request);
  }

  update(id: number, request: UpdateExchangeRateRequest): Observable<ExchangeRate> {
    return this.http.put<ExchangeRate>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
