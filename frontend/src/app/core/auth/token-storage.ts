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
    if (session === null || Date.parse(session.expiresAt) <= Date.now()) {
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

  private parse(raw: string): LoginResponse | null {
    try {
      return JSON.parse(raw) as LoginResponse;
    } catch {
      return null;
    }
  }
}
