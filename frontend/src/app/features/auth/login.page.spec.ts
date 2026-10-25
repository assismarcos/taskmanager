import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { LoginPage } from './login.page';

describe('LoginPage', () => {
  let fixture: ComponentFixture<LoginPage>;
  let controller: HttpTestingController;
  let element: HTMLElement;

  beforeEach(async () => {
    sessionStorage.clear();
    TestBed.configureTestingModule({
      imports: [LoginPage],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    });
    fixture = TestBed.createComponent(LoginPage);
    controller = TestBed.inject(HttpTestingController);
    element = fixture.nativeElement as HTMLElement;
    await fixture.whenStable();
  });

  function type(selector: string, value: string): void {
    const input = element.querySelector<HTMLInputElement>(selector);
    input!.value = value;
    input!.dispatchEvent(new Event('input'));
  }

  async function submit(): Promise<void> {
    element.querySelector<HTMLFormElement>('form')!.dispatchEvent(new Event('submit'));
    await fixture.whenStable();
  }

  it('shows "Invalid email or password" on 401', async () => {
    // Arrange
    type('input[type="email"]', 'ann@mail.com');
    type('input[type="password"]', 'wrong-pass1');

    // Act
    await submit();
    controller
      .expectOne('/api/auth/login')
      .flush({ detail: 'Invalid email or password' }, { status: 401, statusText: 'Unauthorized' });
    await fixture.whenStable();

    // Assert
    expect(element.textContent).toContain('Invalid email or password');
  });

  it('does not call the API while the form is invalid', async () => {
    // Act
    await submit();

    // Assert
    controller.expectNone('/api/auth/login');
    expect(element.textContent).toContain('Email is required');
  });

  it('navigates to /tasks after a successful login', async () => {
    // Arrange
    const navigateByUrl = vi.spyOn(TestBed.inject(Router), 'navigateByUrl').mockResolvedValue(true);
    type('input[type="email"]', 'ann@mail.com');
    type('input[type="password"]', 'secret123');

    // Act
    await submit();
    controller.expectOne('/api/auth/login').flush({
      accessToken: 't',
      expiresAt: new Date(Date.now() + 60_000).toISOString(),
      user: { id: 'u1', name: 'Ann', email: 'ann@mail.com' },
    });

    // Assert
    expect(navigateByUrl).toHaveBeenCalledWith('/tasks');
  });
});
