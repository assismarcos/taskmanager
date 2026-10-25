import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButton } from '@angular/material/button';
import { MatCard, MatCardContent, MatCardHeader, MatCardTitle } from '@angular/material/card';
import { MatError, MatFormField, MatLabel } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-login-page',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatButton,
    MatCard,
    MatCardContent,
    MatCardHeader,
    MatCardTitle,
    MatError,
    MatFormField,
    MatInput,
    MatLabel,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <mat-card class="auth-card">
      <mat-card-header>
        <mat-card-title>Log in</mat-card-title>
      </mat-card-header>
      <mat-card-content>
        <form [formGroup]="form" (ngSubmit)="submit()" novalidate>
          <mat-form-field appearance="outline">
            <mat-label>Email</mat-label>
            <input matInput type="email" formControlName="email" autocomplete="email" />
            @if (form.controls.email.hasError('required')) {
              <mat-error>Email is required</mat-error>
            } @else if (form.controls.email.hasError('email')) {
              <mat-error>Enter a valid email</mat-error>
            }
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Password</mat-label>
            <input
              matInput
              type="password"
              formControlName="password"
              autocomplete="current-password"
            />
            @if (form.controls.password.hasError('required')) {
              <mat-error>Password is required</mat-error>
            }
          </mat-form-field>

          @if (formError()) {
            <p class="form-error" role="alert">{{ formError() }}</p>
          }

          <button mat-flat-button type="submit" [disabled]="submitting()">Log in</button>
        </form>
        <p>No account yet? <a routerLink="/register">Register</a></p>
      </mat-card-content>
    </mat-card>
  `,
  styles: `
    .auth-card {
      max-width: 420px;
      margin: 32px auto;
    }

    form {
      display: flex;
      flex-direction: column;
      gap: 8px;
      padding-top: 16px;
    }

    .form-error {
      color: var(--mat-sys-error);
      margin: 0;
    }
  `,
})
export class LoginPage {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  protected readonly submitting = signal(false);
  protected readonly formError = signal<string | null>(null);

  protected readonly form = new FormGroup({
    email: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.email],
    }),
    password: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
  });

  protected submit(): void {
    this.formError.set(null);
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.submitting.set(true);
    this.auth.login(this.form.getRawValue()).subscribe({
      next: () => void this.router.navigateByUrl(this.returnUrl()),
      error: (error: HttpErrorResponse) => {
        this.submitting.set(false);
        if (error.status === 401) {
          this.formError.set('Invalid email or password');
        } else if (error.status === 429) {
          this.formError.set('Too many attempts, try again in a minute');
        }
      },
    });
  }

  private returnUrl(): string {
    const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl');
    // Only same-app paths, so the query string cannot redirect to another site
    return returnUrl?.startsWith('/') && !returnUrl.startsWith('//') ? returnUrl : '/tasks';
  }
}
