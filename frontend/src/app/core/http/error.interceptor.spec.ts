import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ApplicationRef, Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { MatSnackBar } from '@angular/material/snack-bar';
import { provideRouter, Router } from '@angular/router';
import { AuthService } from '../auth/auth.service';
import { errorInterceptor } from './error.interceptor';

@Component({ template: '' })
class DummyPage {}

describe('errorInterceptor', () => {
  let http: HttpClient;
  let controller: HttpTestingController;
  let auth: AuthService;
  let router: Router;
  let open: ReturnType<typeof vi.spyOn>;

  beforeEach(() => {
    sessionStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        provideRouter([
          { path: 'login', component: DummyPage },
          { path: 'tasks', component: DummyPage },
        ]),
        provideHttpClient(withInterceptors([errorInterceptor])),
        provideHttpClientTesting(),
      ],
    });
    http = TestBed.inject(HttpClient);
    controller = TestBed.inject(HttpTestingController);
    auth = TestBed.inject(AuthService);
    router = TestBed.inject(Router);
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

  it('logs out and redirects on 401', async () => {
    // Arrange
    signIn();
    await router.navigateByUrl('/tasks');

    // Act
    http.get('/api/tasks').subscribe({ error: () => undefined });
    controller.expectOne('/api/tasks').flush(null, { status: 401, statusText: 'Unauthorized' });
    await TestBed.inject(ApplicationRef).whenStable();

    // Assert
    expect(auth.isAuthenticated()).toBe(false);
    expect(router.url).toBe('/login?returnUrl=%2Ftasks');
  });

  it('lets a 401 from login reach the page', async () => {
    // Arrange
    let status = 0;
    await router.navigateByUrl('/tasks');

    // Act
    http
      .post('/api/auth/login', {})
      .subscribe({ error: (error: { status: number }) => (status = error.status) });
    controller
      .expectOne('/api/auth/login')
      .flush(null, { status: 401, statusText: 'Unauthorized' });

    // Assert
    expect(status).toBe(401);
    expect(router.url).toBe('/tasks');
  });

  it.each([0, 500, 503])('shows the snackbar on status %i', (status) => {
    // Act
    http.get('/api/tasks').subscribe({ error: () => undefined });
    controller.expectOne('/api/tasks').flush(null, { status, statusText: 'Error' });

    // Assert
    expect(open).toHaveBeenCalledWith('Something went wrong', 'Close', expect.anything());
  });

  it('passes other errors to the page without a snackbar', async () => {
    // Arrange
    let status = 0;
    await router.navigateByUrl('/tasks');

    // Act
    http
      .get('/api/tasks')
      .subscribe({ error: (error: { status: number }) => (status = error.status) });
    controller.expectOne('/api/tasks').flush(null, { status: 404, statusText: 'Not Found' });

    // Assert
    expect(status).toBe(404);
    expect(open).not.toHaveBeenCalled();
    expect(router.url).toBe('/tasks');
  });
});
