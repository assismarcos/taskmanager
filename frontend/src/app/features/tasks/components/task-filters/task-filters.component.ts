import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { MatFormField, MatLabel } from '@angular/material/form-field';
import { MatOption, MatSelect } from '@angular/material/select';
import { TaskStatus } from '../../../../core/models/task.model';

@Component({
  selector: 'app-task-filters',
  imports: [MatFormField, MatLabel, MatSelect, MatOption],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './task-filters.component.html',
})
export class TaskFiltersComponent {
  readonly status = input<TaskStatus | undefined>();
  readonly changed = output<TaskStatus | undefined>();
}
