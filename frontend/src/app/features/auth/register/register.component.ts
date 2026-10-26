import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, ElementRef, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButton } from '@angular/material/button';
import { MatCard, MatCardContent, MatCardHeader, MatCardTitle } from '@angular/material/card';
import { MatError, MatFormField, MatLabel } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { ProblemDetails } from '../../../core/models/problem-details.model';
import { focusFirstInvalid } from '../focus-first-invalid';

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
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss',
})
export class RegisterComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);

  protected readonly submitting = signal(false);
  protected readonly formError = signal<string | null>(null);

  protected readonly form = new FormGroup({
    name: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.pattern(/\S/), Validators.maxLength(100)],
    }),
    email: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(254), Validators.email],
    }),
    password: new FormControl('', {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.minLength(8),
        Validators.pattern(/^(?=.*\p{L})(?=.*\d).*$/u),
      ],
    }),
  });

  protected submit(): void {
    this.formError.set(null);
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      focusFirstInvalid(this.host.nativeElement, this.form.controls);
      return;
    }
    this.submitting.set(true);
    this.auth.register(this.form.getRawValue()).subscribe({
      next: () => {
        this.snackBar.open('Account created, please log in', 'Close', { duration: 5000 });
        void this.router.navigateByUrl('/login');
      },
      error: (error: HttpErrorResponse) => {
        this.submitting.set(false);
        this.showServerError(error);
      },
    });
  }

  private showServerError(error: HttpErrorResponse): void {
    const problem = error.error as ProblemDetails | null;
    const fieldErrors = problem?.errors ?? {};
    if (error.status === 409) {
      this.setServerError('email', 'Email is already registered');
    } else if (error.status === 400 && Object.keys(fieldErrors).length > 0) {
      this.showValidationErrors(fieldErrors);
    } else if (error.status >= 400 && error.status < 500) {
      this.formError.set(problem?.detail ?? 'Registration failed, please try again');
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
