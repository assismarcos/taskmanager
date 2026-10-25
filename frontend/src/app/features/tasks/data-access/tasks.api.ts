import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { PagedResponse } from '../../../core/models/paged-response.model';
import {
  CreateTaskRequest,
  TaskItem,
  TaskListQuery,
  UpdateTaskRequest,
} from '../../../core/models/task.model';

@Injectable({ providedIn: 'root' })
export class TasksApi {
  private readonly http = inject(HttpClient);

  list(query: TaskListQuery): Observable<PagedResponse<TaskItem>> {
    let params = new HttpParams().set('page', query.page).set('pageSize', query.pageSize);
    if (query.status) {
      params = params.set('status', query.status);
    }
    return this.http.get<PagedResponse<TaskItem>>('/api/tasks', { params });
  }

  get(id: string): Observable<TaskItem> {
    return this.http.get<TaskItem>(`/api/tasks/${id}`);
  }

  create(request: CreateTaskRequest): Observable<TaskItem> {
    return this.http.post<TaskItem>('/api/tasks', request);
  }

  update(id: string, request: UpdateTaskRequest): Observable<TaskItem> {
    return this.http.put<TaskItem>(`/api/tasks/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`/api/tasks/${id}`);
  }
}
