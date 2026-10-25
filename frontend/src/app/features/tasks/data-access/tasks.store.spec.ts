import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { TaskItem } from '../../../core/models/task.model';
import { TasksStore } from './tasks.store';

function task(id: string): TaskItem {
  return {
    id,
    title: `Task ${id}`,
    description: null,
    status: 'Todo',
    dueDate: '2030-01-01',
    isOverdue: false,
    createdAt: '2026-10-25T10:00:00+00:00',
    updatedAt: '2026-10-25T10:00:00+00:00',
  };
}

describe('TasksStore', () => {
  let store: TasksStore;
  let controller: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    store = TestBed.inject(TasksStore);
    controller = TestBed.inject(HttpTestingController);
  });

  function flushList(items: TaskItem[], totalCount: number, page = 1): void {
    controller
      .expectOne((r) => r.url === '/api/tasks' && r.method === 'GET')
      .flush({ items, page, pageSize: 10, totalCount });
  }

  it('load sets tasks and totalCount', () => {
    // AC-7
    // Arrange
    // Act
    store.load();
    flushList([task('a'), task('b')], 12);

    // Assert
    expect(store.tasks().map((t) => t.id)).toEqual(['a', 'b']);
    expect(store.totalCount()).toBe(12);
  });

  it('setStatusFilter reloads from page 1', () => {
    // AC-7
    // Arrange
    store.setPage(3, 10);
    flushList([], 30, 3);

    // Act
    store.setStatusFilter('Done');

    // Assert
    const request = controller.expectOne((r) => r.url === '/api/tasks');
    expect(request.request.params.get('status')).toBe('Done');
    expect(request.request.params.get('page')).toBe('1');
    request.flush({ items: [], page: 1, pageSize: 10, totalCount: 0 });
  });

  it('remove removes the task after the API confirms', () => {
    // Arrange
    store.load();
    flushList([task('a'), task('b')], 2);

    // Act
    store.remove('a').subscribe();
    expect(store.tasks()).toHaveLength(2);
    controller.expectOne('/api/tasks/a').flush(null, { status: 204, statusText: 'No Content' });

    // Assert
    expect(store.tasks().map((t) => t.id)).toEqual(['b']);
    expect(store.totalCount()).toBe(1);
  });
});
