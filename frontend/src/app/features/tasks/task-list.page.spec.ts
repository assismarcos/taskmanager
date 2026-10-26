import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { TasksStore } from './data-access/tasks.store';
import { TaskListPage } from './task-list.page';

describe('TaskListPage', () => {
  it('starts with a fresh store for every instance', async () => {
    // Arrange
    TestBed.configureTestingModule({
      imports: [TaskListPage],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    });
    const controller = TestBed.inject(HttpTestingController);
    const first = TestBed.createComponent(TaskListPage);
    first.detectChanges();
    controller.expectOne((r) => r.url === '/api/tasks');
    first.debugElement.injector.get(TasksStore).setStatusFilter('Done');
    controller.expectOne((r) => r.url === '/api/tasks');
    first.destroy();

    // Act
    const second = TestBed.createComponent(TaskListPage);

    // Assert
    expect(second.debugElement.injector.get(TasksStore).status()).toBeUndefined();
  });
});
