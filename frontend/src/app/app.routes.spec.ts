import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { routes } from './app.routes';
import { TokenStorage } from './core/auth/token-storage';

describe('app routes', () => {
  beforeEach(() => {
    sessionStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideRouter(routes), provideHttpClient(), provideHttpClientTesting()],
    });
  });

  afterEach(() => sessionStorage.clear());

  it('redirect unknown paths to /tasks', async () => {
    // Arrange
    signIn();
    const harness = await RouterTestingHarness.create();

    // Act
    await harness.navigateByUrl('/does-not-exist');

    // Assert
    expect(TestBed.inject(Router).url).toBe('/tasks');
  });

  it('send anonymous visitors to /login with the requested URL', async () => {
    // Arrange
    const harness = await RouterTestingHarness.create();

    // Act
    await harness.navigateByUrl('/tasks');

    // Assert
    expect(TestBed.inject(Router).url).toBe('/login?returnUrl=%2Ftasks');
  });

  it('open the register page without a session', async () => {
    // Arrange
    const harness = await RouterTestingHarness.create();

    // Act
    await harness.navigateByUrl('/register');

    // Assert
    expect(TestBed.inject(Router).url).toBe('/register');
  });
});

function signIn(): void {
  TestBed.inject(TokenStorage).write({
    accessToken: 'token',
    expiresAt: new Date(Date.now() + 60 * 60 * 1000).toISOString(),
    user: { id: '1', name: 'Ana', email: 'ana@example.com' },
  });
}
