import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../auth/auth.service';

const LOGIN_URL = '/api/auth/login';

export const errorInterceptor: HttpInterceptorFn = (request, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const snackBar = inject(MatSnackBar);

  return next(request).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && request.url !== LOGIN_URL) {
        auth.logout();
        void router.navigate(['/login']);
      } else if (error.status === 0 || error.status >= 500) {
        snackBar.open('Something went wrong', 'Close', { duration: 5000 });
      }
      return throwError(() => error);
    }),
  );
};
