import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { MatFormField, MatLabel } from '@angular/material/form-field';
import { MatOption, MatSelect } from '@angular/material/select';
import { TaskStatus } from '../../../core/models/task.model';

@Component({
  selector: 'app-task-filters',
  imports: [MatFormField, MatLabel, MatSelect, MatOption],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <mat-form-field appearance="outline">
      <mat-label>Status</mat-label>
      <mat-select
        [value]="status() ?? ''"
        (selectionChange)="changed.emit($event.value || undefined)"
      >
        <mat-option value="">All</mat-option>
        <mat-option value="Todo">To do</mat-option>
        <mat-option value="InProgress">In progress</mat-option>
        <mat-option value="Done">Done</mat-option>
      </mat-select>
    </mat-form-field>
  `,
})
export class TaskFilters {
  readonly status = input<TaskStatus | undefined>();
  readonly changed = output<TaskStatus | undefined>();
}
