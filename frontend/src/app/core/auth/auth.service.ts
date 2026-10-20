import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LoginRequest, LoginResponse, Session } from '../models/auth.models';

const STORAGE_KEY = 'currency-exchange.session';

/**
 * Maneja la sesión del usuario: login, logout y el JWT.
 * El estado se expone con signals para que la UI reaccione automáticamente.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  private readonly session = signal<Session | null>(this.restoreSession());
  private logoutTimer?: ReturnType<typeof setTimeout>;

  readonly isAuthenticated = computed(() => this.session() !== null);
  readonly isAdmin = computed(() => this.session()?.role === 'Admin');
  readonly username = computed(() => this.session()?.username ?? '');
  readonly role = computed(() => this.session()?.role ?? null);

  constructor() {
    const current = this.session();
    if (current) {
      this.scheduleAutoLogout(current.expiresAt);
    }
  }

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${environment.apiUrl}/api/auth/login`, request).pipe(
      tap((response) => {
        const session: Session = {
          token: response.accessToken,
          username: response.username,
          role: response.role,
          expiresAt: response.expiresAt,
        };
        this.saveSession(session);
        this.scheduleAutoLogout(session.expiresAt);
      }),
    );
  }

  logout(reason?: 'expired'): void {
    clearTimeout(this.logoutTimer);
    this.session.set(null);
    localStorage.removeItem(STORAGE_KEY);
    this.router.navigate(['/login'], { queryParams: reason ? { reason } : {} });
  }

  /** Token vigente, o null si no hay sesión o ya expiró. */
  getToken(): string | null {
    const current = this.session();
    if (!current || isExpired(current.expiresAt)) {
      return null;
    }
    return current.token;
  }

  private saveSession(session: Session): void {
    this.session.set(session);
    localStorage.setItem(STORAGE_KEY, JSON.stringify(session));
  }

  /** Recupera la sesión al recargar la página, descartándola si el token expiró. */
  private restoreSession(): Session | null {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      if (!raw) return null;

      const session = JSON.parse(raw) as Session;
      if (isExpired(session.expiresAt)) {
        localStorage.removeItem(STORAGE_KEY);
        return null;
      }
      return session;
    } catch {
      localStorage.removeItem(STORAGE_KEY);
      return null;
    }
  }

  /** Cierra la sesión automáticamente cuando el token expira. */
  private scheduleAutoLogout(expiresAt: string): void {
    clearTimeout(this.logoutTimer);
    const msUntilExpiry = new Date(expiresAt).getTime() - Date.now();
    this.logoutTimer = setTimeout(() => this.logout('expired'), Math.max(msUntilExpiry, 0));
  }
}

function isExpired(expiresAt: string): boolean {
  return new Date(expiresAt).getTime() <= Date.now();
}
