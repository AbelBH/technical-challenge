import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ConversionRequest, ConversionResponse } from '../models/conversion.models';

@Injectable({ providedIn: 'root' })
export class ConversionService {
  private readonly http = inject(HttpClient);

  convert(request: ConversionRequest): Observable<ConversionResponse> {
    return this.http.post<ConversionResponse>(`${environment.apiUrl}/api/conversions`, request);
  }
}
