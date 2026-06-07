import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResponse, CurrentUser, LoginRequest, RegisterRequest } from './auth.models';

const STORAGE_KEY = 'polla.auth';

/**
 * Holds auth state in signals and persists the session to localStorage.
 * JWT is stateless, so the client just stores the token and derives identity from it.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly base = environment.apiBaseUrl;

  private readonly _user = signal<CurrentUser | null>(this.restore());

  readonly user = this._user.asReadonly();
  readonly isAuthenticated = computed(() => this._user() !== null && !this.isExpired(this._user()));
  readonly isAdmin = computed(() => this._user()?.role === 'Admin');
  readonly displayName = computed(() => this._user()?.displayName ?? '');

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.base}/auth/register`, request)
      .pipe(tap((res) => this.persist(res)));
  }

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.base}/auth/login`, request)
      .pipe(tap((res) => this.persist(res)));
  }

  logout(): void {
    localStorage.removeItem(STORAGE_KEY);
    this._user.set(null);
  }

  get token(): string | null {
    const u = this._user();
    return u && !this.isExpired(u) ? u.token : null;
  }

  // --- internals ---------------------------------------------------------

  private persist(res: AuthResponse): void {
    const user: CurrentUser = {
      token: res.token,
      role: res.role,
      displayName: res.displayName,
      expiresAtUtc: res.expiresAtUtc,
      userId: this.readUserId(res.token),
    };
    localStorage.setItem(STORAGE_KEY, JSON.stringify(user));
    this._user.set(user);
  }

  private restore(): CurrentUser | null {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return null;
    try {
      const user = JSON.parse(raw) as CurrentUser;
      return this.isExpired(user) ? null : user;
    } catch {
      return null;
    }
  }

  private isExpired(user: CurrentUser | null): boolean {
    if (!user) return true;
    return new Date(user.expiresAtUtc).getTime() <= Date.now();
  }

  /** Extracts the user id (sub / nameidentifier) from the JWT payload. */
  private readUserId(token: string): string {
    try {
      const payload = JSON.parse(atob(token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')));
      return (
        payload['sub'] ??
        payload['nameid'] ??
        payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] ??
        ''
      );
    } catch {
      return '';
    }
  }
}
