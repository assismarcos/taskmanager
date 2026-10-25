import { TestBed } from '@angular/core/testing';
import { LoginResponse } from '../models/user.model';
import { TokenStorage } from './token-storage';

const KEY = 'tm.session';

function sessionExpiringAt(expiresAt: string): LoginResponse {
  return {
    accessToken: 'token-1',
    expiresAt,
    user: { id: 'u1', name: 'Ann Lee', email: 'ann@mail.com' },
  };
}

describe('TokenStorage', () => {
  let storage: TokenStorage;

  beforeEach(() => {
    sessionStorage.clear();
    storage = TestBed.inject(TokenStorage);
  });

  it('reads back a session that was written', () => {
    // Arrange
    const session = sessionExpiringAt(new Date(Date.now() + 60_000).toISOString());

    // Act
    storage.write(session);

    // Assert
    expect(storage.read()).toEqual(session);
  });

  it('treats an expired session as empty', () => {
    // Arrange
    sessionStorage.setItem(KEY, JSON.stringify(sessionExpiringAt('2020-01-01T00:00:00+00:00')));

    // Act
    const result = storage.read();

    // Assert
    expect(result).toBeNull();
    expect(sessionStorage.getItem(KEY)).toBeNull();
  });

  it.each(['{}', '{"accessToken":"x"}', '123'])(
    'treats a session without a valid expiry as empty (%s)',
    (raw) => {
      // Arrange
      sessionStorage.setItem(KEY, raw);

      // Act
      const result = storage.read();

      // Assert
      expect(result).toBeNull();
      expect(sessionStorage.getItem(KEY)).toBeNull();
    },
  );

  it('treats corrupt data as empty', () => {
    // Arrange
    sessionStorage.setItem(KEY, '{not json');

    // Act / Assert
    expect(storage.read()).toBeNull();
  });

  it('clear removes the session', () => {
    // Arrange
    storage.write(sessionExpiringAt(new Date(Date.now() + 60_000).toISOString()));

    // Act
    storage.clear();

    // Assert
    expect(storage.read()).toBeNull();
  });
});
