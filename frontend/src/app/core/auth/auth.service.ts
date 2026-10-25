import { Injectable, Signal } from '@angular/core';
import { Observable } from 'rxjs';
import { LoginRequest, LoginResponse, RegisterRequest, User } from '../models/user.model';

@Injectable({ providedIn: 'root' })
export class AuthService {
  readonly currentUser: Signal<User | null> = null as unknown as Signal<User | null>;
  readonly isAuthenticated: Signal<boolean> = null as unknown as Signal<boolean>;

  login(request: LoginRequest): Observable<LoginResponse> {
    throw new Error('not implemented');
  }

  register(request: RegisterRequest): Observable<User> {
    throw new Error('not implemented');
  }

  logout(): void {
    throw new Error('not implemented');
  }

  accessToken(): string | null {
    throw new Error('not implemented');
  }
}
