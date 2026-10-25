import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatSnackBar } from '@angular/material/snack-bar';
import { provideRouter, Router } from '@angular/router';
import { RegisterPage } from './register.page';

@Component({ template: '' })
class DummyPage {}

describe('RegisterPage', () => {
  let fixture: ComponentFixture<RegisterPage>;
  let controller: HttpTestingController;
  let router: Router;
  let element: HTMLElement;

  beforeEach(async () => {
    sessionStorage.clear();
    TestBed.configureTestingModule({
      imports: [RegisterPage],
      providers: [
        provideRouter([{ path: 'login', component: DummyPage }]),
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });
    fixture = TestBed.createComponent(RegisterPage);
    controller = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);
    element = fixture.nativeElement as HTMLElement;
    await fixture.whenStable();
  });

  function type(selector: string, value: string): void {
    const input = element.querySelector<HTMLInputElement>(selector);
    input!.value = value;
    input!.dispatchEvent(new Event('input'));
  }

  function fillValid(): void {
    type('input[formControlName="name"]', 'Ann Lee');
    type('input[type="email"]', 'ann@mail.com');
    type('input[type="password"]', 'secret123');
  }

  async function submit(): Promise<void> {
    element.querySelector<HTMLFormElement>('form')!.dispatchEvent(new Event('submit'));
    await fixture.whenStable();
  }

  async function failRegister(status: number, body: object | null): Promise<void> {
    controller.expectOne('/api/auth/register').flush(body, { status, statusText: 'Error' });
    await fixture.whenStable();
  }

  function fieldOf(selector: string): HTMLElement {
    return element.querySelector(selector)!.closest('mat-form-field')!;
  }

  it('shows a validation message when the password has no digit', async () => {
    // Arrange
    fillValid();
    type('input[type="password"]', 'onlyletters');

    // Act
    await submit();

    // Assert
    expect(element.textContent).toContain('Password needs at least one letter and one digit');
    controller.expectNone('/api/auth/register');
  });

  it('accepts a password with a non-ASCII letter', async () => {
    // Arrange
    fillValid();
    type('input[type="password"]', 'Пароль12');

    // Act
    await submit();

    // Assert
    controller.expectOne('/api/auth/register');
  });

  it('rejects a name made only of whitespace', async () => {
    // Arrange
    fillValid();
    type('input[formControlName="name"]', '   ');

    // Act
    await submit();

    // Assert
    expect(fieldOf('input[formControlName="name"]').textContent).toContain('Name is required');
    controller.expectNone('/api/auth/register');
  });

  it('shows the 254-character limit message for a very long email', async () => {
    // Arrange
    fillValid();
    type('input[type="email"]', `${'a'.repeat(250)}@mail.com`);

    // Act
    await submit();

    // Assert
    expect(fieldOf('input[type="email"]').textContent).toContain(
      'Email must be at most 254 characters',
    );
  });

  it('focuses the first invalid field on submit', async () => {
    // Arrange
    type('input[formControlName="name"]', 'Ann Lee');

    // Act
    await submit();

    // Assert
    expect(document.activeElement).toBe(element.querySelector('input[type="email"]'));
  });

  it('shows the email error on 409', async () => {
    // Arrange
    fillValid();

    // Act
    await submit();
    await failRegister(409, { detail: 'taken' });

    // Assert
    const emailError = fieldOf('input[type="email"]').querySelector('mat-error');
    expect(emailError?.textContent).toContain('Email is already registered');
    expect(element.querySelector('input[type="email"]')?.getAttribute('aria-invalid')).toBe('true');
  });

  it('shows the field message for a 400 with errors', async () => {
    // Arrange
    fillValid();

    // Act
    await submit();
    await failRegister(400, { errors: { name: ['Name is too long'] } });

    // Assert
    expect(fieldOf('input[formControlName="name"]').textContent).toContain('Name is too long');
  });

  it('shows the problem detail for a 400 without errors', async () => {
    // Arrange
    fillValid();

    // Act
    await submit();
    await failRegister(400, { detail: 'Something specific' });

    // Assert
    expect(element.querySelector('[role="alert"]')?.textContent).toContain('Something specific');
  });

  it('shows a fallback message when the error has no detail', async () => {
    // Arrange
    fillValid();

    // Act
    await submit();
    await failRegister(422, null);

    // Assert
    expect(element.querySelector('[role="alert"]')?.textContent).toContain(
      'Registration failed, please try again',
    );
  });

  it('navigates to /login after registering', async () => {
    // Arrange
    const open = vi.spyOn(TestBed.inject(MatSnackBar), 'open');
    fillValid();

    // Act
    await submit();
    controller
      .expectOne('/api/auth/register')
      .flush(
        { id: 'u1', name: 'Ann Lee', email: 'ann@mail.com' },
        { status: 201, statusText: 'Created' },
      );
    await fixture.whenStable();

    // Assert
    expect(router.url).toBe('/login');
    expect(open).toHaveBeenCalledWith('Account created, please log in', 'Close', expect.anything());
  });
});
