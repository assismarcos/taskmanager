import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { TaskItem } from '../../../core/models/task.model';
import { TaskFormComponent } from './task-form.component';

interface FormAccess {
  controls: { dueDate: { setValue(value: Date): void } };
}

describe('TaskFormComponent', () => {
  let fixture: ComponentFixture<TaskFormComponent>;
  let controller: HttpTestingController;
  let element: HTMLElement;

  async function render(id?: string): Promise<void> {
    TestBed.configureTestingModule({
      imports: [TaskFormComponent],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: convertToParamMap(id ? { id } : {}) } },
        },
      ],
    });
    controller = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(TaskFormComponent);
    element = fixture.nativeElement as HTMLElement;
    await fixture.whenStable();
  }

  function setDueDate(date: Date): void {
    (fixture.componentInstance as unknown as { form: FormAccess }).form.controls.dueDate.setValue(
      date,
    );
  }

  function type(selector: string, value: string): void {
    const input = element.querySelector<HTMLInputElement>(selector);
    input!.value = value;
    input!.dispatchEvent(new Event('input'));
  }

  async function submit(): Promise<void> {
    element.querySelector<HTMLFormElement>('form')!.dispatchEvent(new Event('submit'));
    await fixture.whenStable();
    fixture.detectChanges();
  }

  function daysFromTodayUtc(days: number): Date {
    const now = new Date();
    return new Date(now.getUTCFullYear(), now.getUTCMonth(), now.getUTCDate() + days);
  }

  it('shows a validation message when the title is too short', async () => {
    // AC-4
    // Arrange
    await render();
    type('input[formControlName="title"]', 'ab');
    setDueDate(daysFromTodayUtc(5));

    // Act
    await submit();

    // Assert
    controller.expectNone('/api/tasks');
    expect(element.textContent).toContain('Title must be between 3 and 100 characters');
  });

  it('blocks a past due date on create', async () => {
    // AC-5
    // Arrange
    await render();
    type('input[formControlName="title"]', 'Prepare the demo');
    setDueDate(daysFromTodayUtc(-2));

    // Act
    await submit();

    // Assert
    controller.expectNone('/api/tasks');
    expect(element.textContent).toContain('Due date cannot be in the past');
  });

  it('offers only Done and Todo when editing a Done task', async () => {
    // AC-6
    // Arrange
    await render('t1');
    const done: TaskItem = {
      id: 't1',
      title: 'Prepare the demo',
      description: null,
      status: 'Done',
      dueDate: '2020-01-01',
      isOverdue: false,
      createdAt: '2026-10-25T10:00:00+00:00',
      updatedAt: '2026-10-25T10:00:00+00:00',
    };

    // Act
    controller.expectOne('/api/tasks/t1').flush(done);
    await fixture.whenStable();
    fixture.detectChanges();

    // Assert
    const options = Array.from(element.querySelectorAll<HTMLOptionElement>('select option')).map(
      (o) => o.value,
    );
    expect(options).toEqual(['Todo', 'Done']);
  });

  it('maps 400 field errors to form controls', async () => {
    // AC-4, AC-5
    // Arrange
    await render();
    type('input[formControlName="title"]', 'Prepare the demo');
    setDueDate(daysFromTodayUtc(5));
    await submit();

    // Act
    controller.expectOne('/api/tasks').flush(
      {
        errors: {
          title: ['Title is taken.'],
          dueDate: ['Due date cannot be in the past.'],
          other: ['Something else.'],
        },
      },
      { status: 400, statusText: 'Bad Request' },
    );
    await fixture.whenStable();
    fixture.detectChanges();

    // Assert
    const text = element.textContent;
    expect(text).toContain('Title is taken.');
    expect(text).toContain('Due date cannot be in the past.');
    expect(text).toContain('Something else.');
  });
});
