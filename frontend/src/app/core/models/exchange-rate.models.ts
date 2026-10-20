export interface ExchangeRate {
  id: number;
  sourceCurrency: string;
  targetCurrency: string;
  rate: number;
  updatedAt: string;
}

export interface CreateExchangeRateRequest {
  sourceCurrency: string;
  targetCurrency: string;
  rate: number;
  createInverse: boolean;
}

export interface UpdateExchangeRateRequest {
  rate: number;
}
