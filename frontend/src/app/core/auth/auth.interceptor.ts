import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';

/**
 * Agrega el header "Authorization: Bearer <token>" a cada petición hacia la API.
 * Solo se envía a nuestra API, nunca a dominios de terceros.
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = inject(AuthService).getToken();
  const isApiRequest = req.url.startsWith(`${environment.apiUrl}/api/`);

  if (!token || !isApiRequest) {
    return next(req);
  }

  return next(req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }));
};
