import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../environments/environment';
import { authInterceptor } from './auth.interceptor';
import { AuthService } from './auth.service';

describe('authInterceptor', () => {
  let http: HttpClient;
  let httpMock: HttpTestingController;
  let token: string | null;

  beforeEach(() => {
    token = 'test-token';

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        { provide: AuthService, useValue: { getToken: () => token } },
      ],
    });

    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('agrega el header Authorization a las peticiones de la API', () => {
    http.get(`${environment.apiUrl}/api/exchange-rates`).subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}/api/exchange-rates`);
    expect(req.request.headers.get('Authorization')).toBe('Bearer test-token');
    req.flush([]);
  });

  it('no envía el token a dominios de terceros', () => {
    http.get('https://otro-dominio.com/api/datos').subscribe();

    const req = httpMock.expectOne('https://otro-dominio.com/api/datos');
    expect(req.request.headers.has('Authorization')).toBe(false);
    req.flush({});
  });

  it('no agrega el header si no hay sesión', () => {
    token = null;
    http.get(`${environment.apiUrl}/api/exchange-rates`).subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}/api/exchange-rates`);
    expect(req.request.headers.has('Authorization')).toBe(false);
    req.flush([]);
  });
});
