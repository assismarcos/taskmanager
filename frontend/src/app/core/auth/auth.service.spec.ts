import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { LoginResponse } from '../models/user.model';
import { AuthService } from './auth.service';

const response: LoginResponse = {
  accessToken: 'token-1',
  expiresAt: new Date(Date.now() + 60 * 60_000).toISOString(),
  user: { id: 'u1', name: 'Ann Lee', email: 'ann@mail.com' },
};

describe('AuthService', () => {
  let service: AuthService;
  let http: HttpTestingController;

  beforeEach(() => {
    sessionStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(AuthService);
    http = TestBed.inject(HttpTestingController);
  });

  function login(): void {
    service.login({ email: 'ann@mail.com', password: 'secret123' }).subscribe();
    http.expectOne('/api/auth/login').flush(response);
  }

  it('login stores the session and sets currentUser', () => {
    // Act
    login();

    // Assert
    expect(service.currentUser()).toEqual(response.user);
    expect(service.isAuthenticated()).toBe(true);
    expect(service.accessToken()).toBe('token-1');
    expect(JSON.parse(sessionStorage.getItem('tm.session') ?? 'null')).toEqual(response);
  });

  it('logout clears the session', () => {
    // Arrange
    login();

    // Act
    service.logout();

    // Assert
    expect(service.currentUser()).toBeNull();
    expect(service.isAuthenticated()).toBe(false);
    expect(service.accessToken()).toBeNull();
    expect(sessionStorage.getItem('tm.session')).toBeNull();
  });

  it('restores the session from sessionStorage', () => {
    // Arrange
    sessionStorage.setItem('tm.session', JSON.stringify(response));
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });

    // Act
    const restored = TestBed.inject(AuthService);

    // Assert
    expect(restored.currentUser()).toEqual(response.user);
  });

  it('register posts the new user and does not start a session', () => {
    // Arrange
    const request = { name: 'Ann Lee', email: 'ann@mail.com', password: 'secret123' };

    // Act
    service.register(request).subscribe();

    // Assert
    const call = http.expectOne('/api/auth/register');
    expect(call.request.body).toEqual(request);
    call.flush(response.user, { status: 201, statusText: 'Created' });
    expect(service.isAuthenticated()).toBe(false);
  });
});
