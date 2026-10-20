import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../auth/auth.service';

/**
 * Manejo global de errores HTTP: si la API responde 401 (token inválido o expirado)
 * se cierra la sesión y se redirige al login. El resto de errores se propaga al componente.
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);

  return next(req).pipe(
    catchError((error: unknown) => {
      const isLoginRequest = req.url.endsWith('/api/auth/login');

      if (error instanceof HttpErrorResponse && error.status === 401 && !isLoginRequest) {
        auth.logout('expired');
      }

      return throwError(() => error);
    }),
  );
};
