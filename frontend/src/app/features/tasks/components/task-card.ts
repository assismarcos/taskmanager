import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { TaskItem } from '../../../core/models/task.model';

@Component({
  selector: 'app-task-card',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: ``,
})
export class TaskCard {
  readonly task = input.required<TaskItem>();
}
