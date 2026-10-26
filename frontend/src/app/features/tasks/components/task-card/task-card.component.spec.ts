import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { TaskItem } from '../../../../core/models/task.model';
import { TaskCardComponent } from './task-card.component';

describe('TaskCardComponent', () => {
  function render(isOverdue: boolean): HTMLElement {
    TestBed.configureTestingModule({ providers: [provideRouter([])] });
    const fixture = TestBed.createComponent(TaskCardComponent);
    const task: TaskItem = {
      id: 't1',
      title: 'Prepare the demo',
      description: null,
      status: 'Todo',
      dueDate: '2020-01-01',
      isOverdue,
      createdAt: '2026-10-25T10:00:00+00:00',
      updatedAt: '2026-10-25T10:00:00+00:00',
    };
    fixture.componentRef.setInput('task', task);
    fixture.detectChanges();
    return fixture.nativeElement as HTMLElement;
  }

  it('shows the overdue badge when isOverdue is true', () => {
    // AC-8
    // Arrange
    // Act
    const element = render(true);

    // Assert
    expect(element.textContent).toContain('Overdue');
  });

  it('hides the overdue badge when isOverdue is false', () => {
    // AC-8
    // Arrange
    // Act
    const element = render(false);

    // Assert
    expect(element.textContent).not.toContain('Overdue');
  });
});
