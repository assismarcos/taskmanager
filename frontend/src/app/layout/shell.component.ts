import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MatToolbar } from '@angular/material/toolbar';
import { Router, RouterLink, RouterOutlet } from '@angular/router';
import { AuthService } from '../core/auth/auth.service';

@Component({
  selector: 'app-shell',
  imports: [MatButton, MatToolbar, RouterLink, RouterOutlet],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <mat-toolbar>
      <a class="brand" routerLink="/tasks">Task Manager</a>
      <span class="spacer"></span>
      @if (auth.currentUser(); as user) {
        <span class="user-name">{{ user.name }}</span>
        <button mat-button type="button" class="logout" (click)="logout()">Log out</button>
      }
    </mat-toolbar>
    <main class="content">
      <router-outlet />
    </main>
  `,
  styles: `
    .brand {
      color: inherit;
      text-decoration: none;
    }

    .spacer {
      flex: 1;
    }

    .user-name {
      margin-right: 8px;
    }

    .content {
      max-width: 960px;
      margin: 0 auto;
      padding: 16px;
    }
  `,
})
export class ShellComponent {
  protected readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  protected logout(): void {
    this.auth.logout();
    void this.router.navigate(['/login']);
  }
}
