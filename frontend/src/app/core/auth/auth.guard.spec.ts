import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import {
  ActivatedRouteSnapshot,
  provideRouter,
  Router,
  RouterStateSnapshot,
  UrlTree,
} from '@angular/router';
import { authGuard } from './auth.guard';
import { AuthService } from './auth.service';
import { TokenStorage } from './token-storage';

function runGuard(url: string): unknown {
  return TestBed.runInInjectionContext(() =>
    authGuard({} as ActivatedRouteSnapshot, { url } as RouterStateSnapshot),
  );
}

function configure(): void {
  TestBed.configureTestingModule({
    providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
  });
}

function storeSessionExpiringIn(milliseconds: number): void {
  TestBed.inject(TokenStorage).write({
    accessToken: 't',
    expiresAt: new Date(Date.now() + milliseconds).toISOString(),
    user: { id: 'u1', name: 'Ann', email: 'ann@mail.com' },
  });
}

describe('authGuard', () => {
  beforeEach(() => {
    sessionStorage.clear();
    configure();
  });

  it('redirects to /login with returnUrl when not authenticated', () => {
    // Act
    const result = runGuard('/tasks?page=2');

    // Assert
    expect(result).toBeInstanceOf(UrlTree);
    expect(TestBed.inject(Router).serializeUrl(result as UrlTree)).toBe(
      '/login?returnUrl=%2Ftasks%3Fpage%3D2',
    );
  });

  it('allows navigation when authenticated', () => {
    // Arrange
    storeSessionExpiringIn(60_000);
    TestBed.resetTestingModule();
    configure();

    // Act
    const result = runGuard('/tasks');

    // Assert
    expect(result).toBe(true);
  });

  it('redirects and clears the session once it has expired', () => {
    // Arrange
    vi.useFakeTimers({ toFake: ['Date'] });
    try {
      storeSessionExpiringIn(60_000);
      TestBed.resetTestingModule();
      configure();
      TestBed.inject(AuthService);
      vi.setSystemTime(Date.now() + 2 * 60_000);

      // Act
      const result = runGuard('/tasks');

      // Assert
      expect(result).toBeInstanceOf(UrlTree);
      expect(sessionStorage.getItem('tm.session')).toBeNull();
    } finally {
      vi.useRealTimers();
    }
  });
});
