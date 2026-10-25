import { HttpClient } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { LoginRequest, LoginResponse, RegisterRequest, User } from '../models/user.model';
import { LOGIN_URL } from './login-url';
import { TokenStorage } from './token-storage';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly storage = inject(TokenStorage);
  private readonly session = signal<LoginResponse | null>(this.storage.read());

  readonly currentUser = computed(() => this.session()?.user ?? null);
  readonly isAuthenticated = computed(() => this.session() !== null);

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(LOGIN_URL, request).pipe(
      tap((response) => {
        this.storage.write(response);
        this.session.set(response);
      }),
    );
  }

  register(request: RegisterRequest): Observable<User> {
    return this.http.post<User>('/api/auth/register', request);
  }

  logout(): void {
    this.storage.clear();
    this.session.set(null);
  }

  /** Logs out when the token has expired, so callers never see a stale session (ADR-0010). */
  hasActiveSession(): boolean {
    const session = this.session();
    if (session !== null && Date.parse(session.expiresAt) <= Date.now()) {
      this.logout();
    }
    return this.session() !== null;
  }

  accessToken(): string | null {
    return this.hasActiveSession() ? (this.session()?.accessToken ?? null) : null;
  }
}
