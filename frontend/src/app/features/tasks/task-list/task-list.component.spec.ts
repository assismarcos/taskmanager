import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { TasksStore } from '../data-access/tasks.store';
import { TaskListComponent } from './task-list.component';

describe('TaskListComponent', () => {
  it('starts with a fresh store for every instance', async () => {
    // Arrange
    TestBed.configureTestingModule({
      imports: [TaskListComponent],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    });
    const controller = TestBed.inject(HttpTestingController);
    const first = TestBed.createComponent(TaskListComponent);
    first.detectChanges();
    controller.expectOne((r) => r.url === '/api/tasks');
    first.debugElement.injector.get(TasksStore).setStatusFilter('Done');
    controller.expectOne((r) => r.url === '/api/tasks');
    first.destroy();

    // Act
    const second = TestBed.createComponent(TaskListComponent);

    // Assert
    expect(second.debugElement.injector.get(TasksStore).status()).toBeUndefined();
  });
});
