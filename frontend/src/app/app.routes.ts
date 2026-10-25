import { Routes } from '@angular/router';
import { ShellComponent } from './layout/shell.component';
import { TasksPlaceholderPage } from './layout/tasks-placeholder.page';

export const routes: Routes = [
  {
    path: '',
    component: ShellComponent,
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'tasks' },
      { path: 'tasks', component: TasksPlaceholderPage },
    ],
  },
  { path: '**', redirectTo: 'tasks' },
];
