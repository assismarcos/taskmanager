import { Routes } from '@angular/router';
import { TaskFormComponent } from './task-form/task-form.component';
import { TaskListComponent } from './task-list/task-list.component';

export const TASKS_ROUTES: Routes = [
  { path: '', component: TaskListComponent, title: 'Tasks' },
  { path: 'new', component: TaskFormComponent, title: 'New task' },
  { path: ':id/edit', component: TaskFormComponent, title: 'Edit task' },
];
