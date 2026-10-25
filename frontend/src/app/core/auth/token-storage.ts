import { Injectable } from '@angular/core';
import { LoginResponse } from '../models/user.model';

const KEY = 'tm.session';

@Injectable({ providedIn: 'root' })
export class TokenStorage {
  read(): LoginResponse | null {
    const raw = sessionStorage.getItem(KEY);
    if (raw === null) {
      return null;
    }
    const session = this.parse(raw);
    if (!this.isActive(session)) {
      this.clear();
      return null;
    }
    return session;
  }

  write(session: LoginResponse): void {
    sessionStorage.setItem(KEY, JSON.stringify(session));
  }

  clear(): void {
    sessionStorage.removeItem(KEY);
  }

  private parse(raw: string): unknown {
    try {
      return JSON.parse(raw);
    } catch {
      return null;
    }
  }

  private isActive(value: unknown): value is LoginResponse {
    if (typeof value !== 'object' || value === null) {
      return false;
    }
    const { accessToken, expiresAt, user } = value as Partial<LoginResponse>;
    return (
      typeof accessToken === 'string' &&
      typeof user === 'object' &&
      user !== null &&
      typeof expiresAt === 'string' &&
      Date.parse(expiresAt) > Date.now()
    );
  }
}
