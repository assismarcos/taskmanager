import { Routes } from '@angular/router';
import { LoginPage } from './login.page';
import { RegisterPage } from './register.page';

export const AUTH_ROUTES: Routes = [
  { path: 'login', component: LoginPage, title: 'Log in' },
  { path: 'register', component: RegisterPage, title: 'Register' },
];
