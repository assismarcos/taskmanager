import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
import { ShellComponent } from './layout/shell.component';
import { TasksPlaceholderPage } from './layout/tasks-placeholder.page';

export const routes: Routes = [
  {
    path: '',
    component: ShellComponent,
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'tasks' },
      { path: 'tasks', canActivate: [authGuard], component: TasksPlaceholderPage },
      // After 'tasks', so the auth chunk is not loaded on every /tasks navigation.
      {
        path: '',
        loadChildren: () => import('./features/auth/auth.routes').then((m) => m.AUTH_ROUTES),
      },
    ],
  },
  { path: '**', redirectTo: 'tasks' },
];
