import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { LoginPage } from './login.page';

@Component({ template: '' })
class DummyPage {}

describe('LoginPage', () => {
  let fixture: ComponentFixture<LoginPage>;
  let controller: HttpTestingController;
  let router: Router;
  let element: HTMLElement;

  async function render(returnUrl?: string): Promise<void> {
    sessionStorage.clear();
    TestBed.configureTestingModule({
      imports: [LoginPage],
      providers: [
        provideRouter([
          { path: 'login', component: DummyPage },
          { path: 'tasks', component: DummyPage },
        ]),
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });
    router = TestBed.inject(Router);
    controller = TestBed.inject(HttpTestingController);
    await router.navigate(['/login'], { queryParams: returnUrl ? { returnUrl } : {} });
    fixture = TestBed.createComponent(LoginPage);
    element = fixture.nativeElement as HTMLElement;
    await fixture.whenStable();
  }

  function type(selector: string, value: string): void {
    const input = element.querySelector<HTMLInputElement>(selector);
    input!.value = value;
    input!.dispatchEvent(new Event('input'));
  }

  function fillValid(): void {
    type('input[type="email"]', 'ann@mail.com');
    type('input[type="password"]', 'secret123');
  }

  async function submit(): Promise<void> {
    element.querySelector<HTMLFormElement>('form')!.dispatchEvent(new Event('submit'));
    await fixture.whenStable();
  }

  async function failLogin(status: number, body: object | null): Promise<void> {
    controller.expectOne('/api/auth/login').flush(body, { status, statusText: 'Error' });
    await fixture.whenStable();
  }

  function succeedLogin(): void {
    controller.expectOne('/api/auth/login').flush({
      accessToken: 't',
      expiresAt: new Date(Date.now() + 60_000).toISOString(),
      user: { id: 'u1', name: 'Ann', email: 'ann@mail.com' },
    });
  }

  it('shows "Invalid email or password" on 401', async () => {
    // Arrange
    await render();
    fillValid();

    // Act
    await submit();
    await failLogin(401, { detail: 'Invalid email or password' });

    // Assert
    expect(element.textContent).toContain('Invalid email or password');
  });

  it('shows a retry message on 429', async () => {
    // Arrange
    await render();
    fillValid();

    // Act
    await submit();
    await failLogin(429, null);

    // Assert
    expect(element.textContent).toContain('Too many attempts, try again in a minute');
  });

  it('shows the problem detail or a fallback on other client errors', async () => {
    // Arrange
    await render();
    fillValid();

    // Act
    await submit();
    await failLogin(400, null);

    // Assert
    expect(element.textContent).toContain('Login failed, please try again');
  });

  it('does not call the API while the form is invalid', async () => {
    // Arrange
    await render();

    // Act
    await submit();

    // Assert
    controller.expectNone('/api/auth/login');
    expect(element.textContent).toContain('Email is required');
  });

  it('focuses the first invalid field on submit', async () => {
    // Arrange
    await render();

    // Act
    await submit();

    // Assert
    expect(document.activeElement).toBe(element.querySelector('input[type="email"]'));
  });

  it('navigates to /tasks after a successful login', async () => {
    // Arrange
    await render();
    fillValid();

    // Act
    await submit();
    succeedLogin();
    await fixture.whenStable();

    // Assert
    expect(router.url).toBe('/tasks');
  });

  it('honours a same-app returnUrl', async () => {
    // Arrange
    await render('/tasks?page=2');
    fillValid();

    // Act
    await submit();
    succeedLogin();
    await fixture.whenStable();

    // Assert
    expect(router.url).toBe('/tasks?page=2');
  });

  it.each(['//evil.com', '/\\evil.com', 'https://evil.com'])(
    'ignores the unsafe returnUrl %s',
    async (returnUrl) => {
      // Arrange
      await render(returnUrl);
      fillValid();

      // Act
      await submit();
      succeedLogin();
      await fixture.whenStable();

      // Assert
      expect(router.url).toBe('/tasks');
    },
  );
});
