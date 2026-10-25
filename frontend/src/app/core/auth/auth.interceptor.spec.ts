import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { AuthService } from './auth.service';
import { authInterceptor } from './auth.interceptor';

describe('authInterceptor', () => {
  let http: HttpClient;
  let controller: HttpTestingController;

  beforeEach(() => {
    sessionStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
      ],
    });
    http = TestBed.inject(HttpClient);
    controller = TestBed.inject(HttpTestingController);
  });

  function signIn(): void {
    TestBed.inject(AuthService)
      .login({ email: 'ann@mail.com', password: 'secret123' })
      .subscribe();
    controller.expectOne('/api/auth/login').flush({
      accessToken: 'token-1',
      expiresAt: new Date(Date.now() + 60_000).toISOString(),
      user: { id: 'u1', name: 'Ann', email: 'ann@mail.com' },
    });
  }

  it('adds the bearer token to /api requests', () => {
    // Arrange
    signIn();

    // Act
    http.get('/api/tasks').subscribe();

    // Assert
    const call = controller.expectOne('/api/tasks');
    expect(call.request.headers.get('Authorization')).toBe('Bearer token-1');
  });

  it('does not add a token to requests outside /api', () => {
    // Arrange
    signIn();

    // Act
    http.get('/assets/config.json').subscribe();

    // Assert
    const call = controller.expectOne('/assets/config.json');
    expect(call.request.headers.has('Authorization')).toBe(false);
  });

  it('adds no header when there is no token', () => {
    // Act
    http.get('/api/tasks').subscribe();

    // Assert
    expect(controller.expectOne('/api/tasks').request.headers.has('Authorization')).toBe(false);
  });
});
