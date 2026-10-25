import { HttpErrorResponse } from '@angular/common/http';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  ElementRef,
  inject,
  OnInit,
  signal,
} from '@angular/core';
import {
  AbstractControl,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { MatButton } from '@angular/material/button';
import { provideNativeDateAdapter } from '@angular/material/core';
import {
  MatDatepicker,
  MatDatepickerInput,
  MatDatepickerToggle,
} from '@angular/material/datepicker';
import { MatError, MatFormField, MatHint, MatLabel, MatSuffix } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ProblemDetails } from '../../core/models/problem-details.model';
import { TaskStatus } from '../../core/models/task.model';
import { focusFirstInvalid } from '../auth/focus-first-invalid';
import { TasksStore } from './data-access/tasks.store';
import { toDueDateString, toLocalDate, todayUtc } from './due-date';

const ALL_STATUSES: TaskStatus[] = ['Todo', 'InProgress', 'Done'];
const STATUS_LABELS: Record<TaskStatus, string> = {
  Todo: 'To do',
  InProgress: 'In progress',
  Done: 'Done',
};

function titleRules(control: AbstractControl<string>): ValidationErrors | null {
  const length = control.value.trim().length;
  if (length === 0) {
    return { required: true };
  }
  return length < 3 || length > 100 ? { titleLength: true } : null;
}

function notInThePast(control: AbstractControl<Date | null>): ValidationErrors | null {
  const value = control.value;
  return value && value < todayUtc() ? { pastDate: true } : null;
}

@Component({
  selector: 'app-task-form-page',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatButton,
    MatDatepicker,
    MatDatepickerInput,
    MatDatepickerToggle,
    MatError,
    MatFormField,
    MatHint,
    MatInput,
    MatLabel,
    MatSuffix,
  ],
  providers: [provideNativeDateAdapter()],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <h1>{{ isEdit ? 'Edit task' : 'New task' }}</h1>
    <form [formGroup]="form" (ngSubmit)="submit()" novalidate>
      <mat-form-field appearance="outline">
        <mat-label>Title</mat-label>
        <input matInput formControlName="title" />
        @if (form.controls.title.hasError('server')) {
          <mat-error>{{ form.controls.title.getError('server') }}</mat-error>
        } @else if (form.controls.title.hasError('required')) {
          <mat-error>Title is required</mat-error>
        } @else if (form.controls.title.hasError('titleLength')) {
          <mat-error>Title must be between 3 and 100 characters</mat-error>
        }
      </mat-form-field>

      <mat-form-field appearance="outline">
        <mat-label>Description</mat-label>
        <textarea matInput formControlName="description" rows="4"></textarea>
        <mat-hint align="end">{{ form.controls.description.value.length }} / 1000</mat-hint>
        @if (form.controls.description.hasError('server')) {
          <mat-error>{{ form.controls.description.getError('server') }}</mat-error>
        } @else if (form.controls.description.hasError('maxlength')) {
          <mat-error>Description must be at most 1000 characters</mat-error>
        }
      </mat-form-field>

      <mat-form-field appearance="outline">
        <mat-label>Due date</mat-label>
        <input
          matInput
          formControlName="dueDate"
          [matDatepicker]="picker"
          [min]="isEdit ? null : minDate"
        />
        <mat-datepicker-toggle matIconSuffix [for]="picker" />
        <mat-datepicker #picker />
        @if (form.controls.dueDate.hasError('server')) {
          <mat-error>{{ form.controls.dueDate.getError('server') }}</mat-error>
        } @else if (form.controls.dueDate.hasError('required')) {
          <mat-error>Due date is required</mat-error>
        } @else if (
          form.controls.dueDate.hasError('pastDate') ||
          form.controls.dueDate.hasError('matDatepickerMin')
        ) {
          <mat-error>Due date cannot be in the past</mat-error>
        } @else if (form.controls.dueDate.hasError('matDatepickerParse')) {
          <mat-error>Enter a valid date</mat-error>
        }
      </mat-form-field>

      <mat-form-field appearance="outline">
        <mat-label>Status</mat-label>
        <select matNativeControl formControlName="status">
          @for (status of statusOptions(); track status) {
            <option [value]="status">{{ label(status) }}</option>
          }
        </select>
        @if (form.controls.status.hasError('server')) {
          <mat-error>{{ form.controls.status.getError('server') }}</mat-error>
        }
      </mat-form-field>

      @if (formError()) {
        <p class="form-error" role="alert">{{ formError() }}</p>
      }

      <div class="actions">
        <button mat-flat-button type="submit" [disabled]="submitting()">Save</button>
        <a mat-button routerLink="/tasks">Cancel</a>
      </div>
    </form>
  `,
  styles: `
    form {
      display: flex;
      flex-direction: column;
      max-width: 560px;
    }
    .form-error {
      color: #b71c1c;
    }
    .actions {
      display: flex;
      gap: 8px;
    }
  `,
})
export class TaskFormPage implements OnInit {
  private readonly store = inject(TasksStore);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);
  private readonly id = inject(ActivatedRoute).snapshot.paramMap.get('id');

  protected readonly isEdit = this.id !== null;
  protected readonly minDate = todayUtc();
  protected readonly submitting = signal(false);
  protected readonly formError = signal<string | null>(null);
  private readonly originalStatus = signal<TaskStatus | null>(null);

  // AC-6: a Done task can only go back to Todo
  protected readonly statusOptions = computed<TaskStatus[]>(() =>
    this.originalStatus() === 'Done' ? ['Todo', 'Done'] : ALL_STATUSES,
  );

  protected readonly form = new FormGroup({
    title: new FormControl('', { nonNullable: true, validators: [titleRules] }),
    description: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(1000)],
    }),
    dueDate: new FormControl<Date | null>(null, {
      validators: [Validators.required, ...(this.isEdit ? [] : [notInThePast])],
    }),
    status: new FormControl<TaskStatus>('Todo', { nonNullable: true }),
  });

  ngOnInit(): void {
    if (this.id === null) {
      return;
    }
    this.store.get(this.id).subscribe({
      next: (task) => {
        this.originalStatus.set(task.status);
        this.form.setValue({
          title: task.title,
          description: task.description ?? '',
          dueDate: toLocalDate(task.dueDate),
          status: task.status,
        });
      },
      error: (error: HttpErrorResponse) =>
        this.formError.set(error.status === 404 ? 'Task not found' : 'Could not load the task'),
    });
  }

  protected label(status: TaskStatus): string {
    return STATUS_LABELS[status];
  }

  protected submit(): void {
    this.formError.set(null);
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      focusFirstInvalid(this.host.nativeElement, this.form.controls);
      return;
    }
    const value = this.form.getRawValue();
    const body = {
      title: value.title.trim(),
      description: value.description.trim() || null,
      dueDate: toDueDateString(value.dueDate as Date),
      status: value.status,
    };
    this.submitting.set(true);
    const request = this.id === null ? this.store.create(body) : this.store.update(this.id, body);
    request.subscribe({
      next: () => {
        this.snackBar.open(this.isEdit ? 'Task updated' : 'Task created', 'OK', { duration: 3000 });
        void this.router.navigateByUrl('/tasks');
      },
      error: (error: HttpErrorResponse) => {
        this.submitting.set(false);
        this.showServerErrors(error);
      },
    });
  }

  private showServerErrors(error: HttpErrorResponse): void {
    const problem = error.error as ProblemDetails | null;
    const fieldErrors = problem?.errors;
    if (error.status !== 400 || !fieldErrors) {
      this.formError.set(problem?.detail ?? 'Could not save the task, please try again');
      return;
    }
    const unmatched: string[] = [];
    for (const [key, messages] of Object.entries(fieldErrors)) {
      const name = key.charAt(0).toLowerCase() + key.slice(1);
      const control = (this.form.controls as Record<string, AbstractControl | undefined>)[name];
      if (control) {
        control.setErrors({ server: messages.join(' ') });
        control.markAsTouched();
      } else {
        unmatched.push(...messages);
      }
    }
    if (unmatched.length > 0) {
      this.formError.set(unmatched.join(' '));
    }
  }
}
