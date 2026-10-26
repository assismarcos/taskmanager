import { inject, Injectable, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import {
  CreateTaskRequest,
  TaskItem,
  TaskStatus,
  UpdateTaskRequest,
} from '../../../core/models/task.model';
import { TasksApi } from './tasks.api';

@Injectable()
export class TasksStore {
  private readonly api = inject(TasksApi);

  private readonly _tasks = signal<TaskItem[]>([]);
  private readonly _totalCount = signal(0);
  private readonly _status = signal<TaskStatus | undefined>(undefined);
  private readonly _page = signal(1);
  private readonly _pageSize = signal(10);
  private readonly _loading = signal(false);
  private readonly _error = signal(false);

  readonly tasks = this._tasks.asReadonly();
  readonly totalCount = this._totalCount.asReadonly();
  readonly status = this._status.asReadonly();
  readonly page = this._page.asReadonly();
  readonly pageSize = this._pageSize.asReadonly();
  readonly loading = this._loading.asReadonly();
  readonly error = this._error.asReadonly();

  load(): void {
    this.fetch(false);
  }

  private fetch(stepBackWhenEmpty: boolean): void {
    this._loading.set(true);
    this._error.set(false);
    this.api
      .list({ status: this._status(), page: this._page(), pageSize: this._pageSize() })
      .subscribe({
        next: (response) => {
          if (stepBackWhenEmpty && response.items.length === 0 && this._page() > 1) {
            this._page.update((page) => page - 1);
            this.fetch(false);
            return;
          }
          this._tasks.set(response.items);
          this._totalCount.set(response.totalCount);
          this._loading.set(false);
        },
        error: () => {
          this._error.set(true);
          this._loading.set(false);
        },
      });
  }

  setStatusFilter(status: TaskStatus | undefined): void {
    this._status.set(status);
    this._page.set(1);
    this.load();
  }

  setPage(page: number, pageSize: number): void {
    this._page.set(page);
    this._pageSize.set(pageSize);
    this.load();
  }

  get(id: string): Observable<TaskItem> {
    return this.api.get(id);
  }

  create(request: CreateTaskRequest): Observable<TaskItem> {
    return this.api.create(request);
  }

  update(id: string, request: UpdateTaskRequest): Observable<TaskItem> {
    return this.api.update(id, request);
  }

  remove(id: string): Observable<void> {
    return this.api.delete(id).pipe(tap(() => this.fetch(true)));
  }
}
