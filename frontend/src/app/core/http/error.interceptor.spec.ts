import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { MatSnackBar } from '@angular/material/snack-bar';
import { provideRouter, Router } from '@angular/router';
import { AuthService } from '../auth/auth.service';
import { errorInterceptor } from './error.interceptor';

describe('errorInterceptor', () => {
  let http: HttpClient;
  let controller: HttpTestingController;
  let auth: AuthService;
  let navigate: ReturnType<typeof vi.spyOn>;
  let open: ReturnType<typeof vi.spyOn>;

  beforeEach(() => {
    sessionStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        provideHttpClient(withInterceptors([errorInterceptor])),
        provideHttpClientTesting(),
      ],
    });
    http = TestBed.inject(HttpClient);
    controller = TestBed.inject(HttpTestingController);
    auth = TestBed.inject(AuthService);
    navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
    open = vi.spyOn(TestBed.inject(MatSnackBar), 'open');
  });

  function signIn(): void {
    auth.login({ email: 'ann@mail.com', password: 'secret123' }).subscribe();
    controller.expectOne('/api/auth/login').flush({
      accessToken: 'token-1',
      expiresAt: new Date(Date.now() + 60_000).toISOString(),
      user: { id: 'u1', name: 'Ann', email: 'ann@mail.com' },
    });
  }

  it('logs out and redirects on 401', () => {
    // Arrange
    signIn();

    // Act
    http.get('/api/tasks').subscribe({ error: () => undefined });
    controller.expectOne('/api/tasks').flush(null, { status: 401, statusText: 'Unauthorized' });

    // Assert
    expect(auth.isAuthenticated()).toBe(false);
    expect(navigate).toHaveBeenCalledWith(['/login']);
  });

  it('lets a 401 from login reach the page', () => {
    // Arrange
    let status = 0;

    // Act
    http
      .post('/api/auth/login', {})
      .subscribe({ error: (error: { status: number }) => (status = error.status) });
    controller
      .expectOne('/api/auth/login')
      .flush(null, { status: 401, statusText: 'Unauthorized' });

    // Assert
    expect(status).toBe(401);
    expect(navigate).not.toHaveBeenCalled();
  });

  it.each([0, 500, 503])('shows the snackbar on status %i', (status) => {
    // Act
    http.get('/api/tasks').subscribe({ error: () => undefined });
    controller.expectOne('/api/tasks').flush(null, { status, statusText: 'Error' });

    // Assert
    expect(open).toHaveBeenCalledWith('Something went wrong', 'Close', expect.anything());
  });

  it('passes other errors to the page without a snackbar', () => {
    // Arrange
    let status = 0;

    // Act
    http
      .get('/api/tasks')
      .subscribe({ error: (error: { status: number }) => (status = error.status) });
    controller.expectOne('/api/tasks').flush(null, { status: 404, statusText: 'Not Found' });

    // Assert
    expect(status).toBe(404);
    expect(open).not.toHaveBeenCalled();
    expect(navigate).not.toHaveBeenCalled();
  });
});
