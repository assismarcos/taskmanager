import { Injectable } from '@angular/core';
import { LoginResponse } from '../models/user.model';

@Injectable({ providedIn: 'root' })
export class TokenStorage {
  read(): LoginResponse | null {
    throw new Error('not implemented');
  }

  write(session: LoginResponse): void {
    throw new Error('not implemented');
  }

  clear(): void {
    throw new Error('not implemented');
  }
}
