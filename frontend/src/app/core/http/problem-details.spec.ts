import { HttpErrorResponse } from '@angular/common/http';
import { getErrorMessage } from './problem-details';

describe('getErrorMessage', () => {
  it('usa el "detail" del ProblemDetails', () => {
    const error = new HttpErrorResponse({
      status: 404,
      error: { title: 'Recurso no encontrado', detail: 'No existe el tipo de cambio con Id 9.' },
    });
    expect(getErrorMessage(error)).toBe('No existe el tipo de cambio con Id 9.');
  });

  it('une los errores de validación por campo', () => {
    const error = new HttpErrorResponse({
      status: 400,
      error: { errors: { rate: ['Debe ser mayor a cero.'], amount: ['No puede ser negativo.'] } },
    });
    expect(getErrorMessage(error)).toBe('Debe ser mayor a cero. No puede ser negativo.');
  });

  it('indica cuando la API no está disponible (status 0)', () => {
    const error = new HttpErrorResponse({ status: 0 });
    expect(getErrorMessage(error)).toContain('No se pudo conectar');
  });

  it('devuelve un mensaje genérico para errores que no son HTTP', () => {
    expect(getErrorMessage(new Error('x'))).toBe('Ocurrió un error inesperado.');
  });
});
