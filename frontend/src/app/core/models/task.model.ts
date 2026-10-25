export type TaskStatus = 'Todo' | 'InProgress' | 'Done';

export interface TaskItem {
  id: string;
  title: string;
  description: string | null;
  status: TaskStatus;
  dueDate: string;
  isOverdue: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateTaskRequest {
  title: string;
  description: string | null;
  dueDate: string;
  status?: TaskStatus;
}

export interface UpdateTaskRequest {
  title: string;
  description: string | null;
  dueDate: string;
  status: TaskStatus;
}

export interface TaskListQuery {
  status?: TaskStatus;
  page: number;
  pageSize: number;
}
