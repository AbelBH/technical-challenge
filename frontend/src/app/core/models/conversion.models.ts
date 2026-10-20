export interface ConversionRequest {
  amount: number;
  sourceCurrency: string;
  targetCurrency: string;
}

export interface ConversionResponse {
  amount: number;
  convertedAmount: number;
  sourceCurrency: string;
  targetCurrency: string;
  exchangeRate: number;
}
