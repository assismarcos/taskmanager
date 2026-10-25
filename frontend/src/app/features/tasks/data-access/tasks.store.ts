import { Injectable, signal } from '@angular/core';
import { EMPTY, Observable } from 'rxjs';
import { TaskItem, TaskStatus } from '../../../core/models/task.model';

@Injectable({ providedIn: 'root' })
export class TasksStore {
  readonly tasks = signal<TaskItem[]>([]);
  readonly totalCount = signal(0);

  load(): void {}
  setStatusFilter(_status: TaskStatus | undefined): void {}
  setPage(_page: number, _pageSize: number): void {}
  remove(_id: string): Observable<void> {
    return EMPTY;
  }
}
