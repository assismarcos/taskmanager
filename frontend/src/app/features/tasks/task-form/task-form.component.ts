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
import { ProblemDetails } from '../../../core/models/problem-details.model';
import { TaskStatus } from '../../../core/models/task.model';
import { focusFirstInvalid } from '../../auth/focus-first-invalid';
import { TasksStore } from '../data-access/tasks.store';
import { toDueDateString, toLocalDate, todayUtc } from '../due-date';

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
  providers: [TasksStore, provideNativeDateAdapter()],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './task-form.component.html',
  styleUrl: './task-form.component.scss',
})
export class TaskFormComponent implements OnInit {
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
