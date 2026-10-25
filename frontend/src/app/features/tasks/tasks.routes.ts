import { Routes } from '@angular/router';
import { TaskFormPage } from './task-form.page';
import { TaskListPage } from './task-list.page';

export const TASKS_ROUTES: Routes = [
  { path: '', component: TaskListPage, title: 'Tasks' },
  { path: 'new', component: TaskFormPage, title: 'New task' },
  { path: ':id/edit', component: TaskFormPage, title: 'Edit task' },
];
