import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { TokenStorage } from '../../core/auth/token-storage';
import { ShellComponent } from './shell.component';

@Component({ template: '' })
class DummyPage {}

describe('ShellComponent', () => {
  let element: HTMLElement;
  let fixture: ComponentFixture<ShellComponent>;

  async function render(signedIn: boolean): Promise<void> {
    sessionStorage.clear();
    TestBed.configureTestingModule({
      imports: [ShellComponent],
      providers: [
        provideRouter([{ path: 'login', component: DummyPage }]),
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });
    if (signedIn) {
      TestBed.inject(TokenStorage).write({
        accessToken: 't',
        expiresAt: new Date(Date.now() + 60_000).toISOString(),
        user: { id: 'u1', name: 'Ann Lee', email: 'ann@mail.com' },
      });
    }
    fixture = TestBed.createComponent(ShellComponent);
    element = fixture.nativeElement as HTMLElement;
    await fixture.whenStable();
  }

  function logoutButton(): HTMLButtonElement | undefined {
    return Array.from(element.querySelectorAll('button')).find(
      (button) => button.textContent?.trim() === 'Log out',
    );
  }

  it('renders the toolbar and router outlet', async () => {
    // Act
    await render(false);

    // Assert
    expect(element.querySelector('mat-toolbar')).not.toBeNull();
    expect(element.querySelector('router-outlet')).not.toBeNull();
  });

  it('hides the user name and logout button when not authenticated', async () => {
    // Act
    await render(false);

    // Assert
    expect(element.textContent).not.toContain('Ann Lee');
    expect(logoutButton()).toBeUndefined();
  });

  it('shows the user name and a logout button when authenticated', async () => {
    // Act
    await render(true);

    // Assert
    expect(element.querySelector('mat-toolbar')?.textContent).toContain('Ann Lee');
    expect(logoutButton()).toBeDefined();
  });

  it('logout clears the session and goes to /login', async () => {
    // Arrange
    await render(true);

    // Act
    logoutButton()!.click();
    await fixture.whenStable();

    // Assert
    expect(TestBed.inject(AuthService).isAuthenticated()).toBe(false);
    expect(TestBed.inject(Router).url).toBe('/login');
    expect(logoutButton()).toBeUndefined();
  });
});
