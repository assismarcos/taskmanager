import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { RegisterPage } from './register.page';

describe('RegisterPage', () => {
  let fixture: ComponentFixture<RegisterPage>;
  let controller: HttpTestingController;
  let element: HTMLElement;

  beforeEach(async () => {
    sessionStorage.clear();
    TestBed.configureTestingModule({
      imports: [RegisterPage],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    });
    fixture = TestBed.createComponent(RegisterPage);
    controller = TestBed.inject(HttpTestingController);
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

  it('shows the email error on 409', async () => {
    // Arrange
    fillValid();

    // Act
    await submit();
    controller
      .expectOne('/api/auth/register')
      .flush({ detail: 'taken' }, { status: 409, statusText: 'Conflict' });
    await fixture.whenStable();

    // Assert
    expect(element.textContent).toContain('Email is already registered');
  });

  it('shows the field message for a 400 with errors', async () => {
    // Arrange
    fillValid();

    // Act
    await submit();
    controller
      .expectOne('/api/auth/register')
      .flush(
        { errors: { name: ['Name is too long'] } },
        { status: 400, statusText: 'Bad Request' },
      );
    await fixture.whenStable();

    // Assert
    expect(element.textContent).toContain('Name is too long');
  });
});
