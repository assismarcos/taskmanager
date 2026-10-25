import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButton } from '@angular/material/button';
import { MatCard, MatCardContent, MatCardHeader, MatCardTitle } from '@angular/material/card';
import { MatError, MatFormField, MatLabel } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { ProblemDetails } from '../../core/models/problem-details.model';

@Component({
  selector: 'app-register-page',
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
        <mat-card-title>Create account</mat-card-title>
      </mat-card-header>
      <mat-card-content>
        <form [formGroup]="form" (ngSubmit)="submit()" novalidate>
          <mat-form-field appearance="outline">
            <mat-label>Name</mat-label>
            <input matInput formControlName="name" autocomplete="name" />
            @if (form.controls.name.hasError('required')) {
              <mat-error>Name is required</mat-error>
            } @else if (form.controls.name.hasError('maxlength')) {
              <mat-error>Name must be at most 100 characters</mat-error>
            } @else if (form.controls.name.hasError('server')) {
              <mat-error>{{ form.controls.name.getError('server') }}</mat-error>
            }
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Email</mat-label>
            <input matInput type="email" formControlName="email" autocomplete="email" />
            @if (form.controls.email.hasError('required')) {
              <mat-error>Email is required</mat-error>
            } @else if (form.controls.email.hasError('email')) {
              <mat-error>Enter a valid email</mat-error>
            } @else if (form.controls.email.hasError('maxlength')) {
              <mat-error>Email must be at most 254 characters</mat-error>
            } @else if (form.controls.email.hasError('server')) {
              <mat-error>{{ form.controls.email.getError('server') }}</mat-error>
            }
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Password</mat-label>
            <input
              matInput
              type="password"
              formControlName="password"
              autocomplete="new-password"
            />
            @if (form.controls.password.hasError('required')) {
              <mat-error>Password is required</mat-error>
            } @else if (form.controls.password.hasError('minlength')) {
              <mat-error>Password must be at least 8 characters</mat-error>
            } @else if (form.controls.password.hasError('pattern')) {
              <mat-error>Password needs at least one letter and one digit</mat-error>
            } @else if (form.controls.password.hasError('server')) {
              <mat-error>{{ form.controls.password.getError('server') }}</mat-error>
            }
          </mat-form-field>

          @if (formError()) {
            <p class="form-error" role="alert">{{ formError() }}</p>
          }

          <button mat-flat-button type="submit" [disabled]="submitting()">Register</button>
        </form>
        <p>Already registered? <a routerLink="/login">Log in</a></p>
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
export class RegisterPage {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly submitting = signal(false);
  protected readonly formError = signal<string | null>(null);

  protected readonly form = new FormGroup({
    name: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(100)],
    }),
    email: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.email, Validators.maxLength(254)],
    }),
    password: new FormControl('', {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.minLength(8),
        Validators.pattern(/^(?=.*[A-Za-z])(?=.*\d).*$/),
      ],
    }),
  });

  protected submit(): void {
    this.formError.set(null);
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.submitting.set(true);
    this.auth.register(this.form.getRawValue()).subscribe({
      next: () => void this.router.navigateByUrl('/login'),
      error: (error: HttpErrorResponse) => {
        this.submitting.set(false);
        this.showServerError(error);
      },
    });
  }

  private showServerError(error: HttpErrorResponse): void {
    if (error.status === 409) {
      this.setServerError('email', 'Email is already registered');
    } else if (error.status === 400) {
      this.showValidationErrors((error.error as ProblemDetails | null)?.errors ?? {});
    }
  }

  private showValidationErrors(errors: Record<string, string[]>): void {
    for (const [field, messages] of Object.entries(errors)) {
      if (field === 'name' || field === 'email' || field === 'password') {
        this.setServerError(field, messages[0]);
      } else {
        this.formError.set(messages[0]);
      }
    }
  }

  private setServerError(field: 'name' | 'email' | 'password', message: string): void {
    const control = this.form.controls[field];
    control.setErrors({ server: message });
    control.markAsTouched();
  }
}
