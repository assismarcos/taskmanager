import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { TaskStatus } from '../../core/models/task.model';

const LABELS: Record<TaskStatus, string> = {
  Todo: 'To do',
  InProgress: 'In progress',
  Done: 'Done',
};

@Component({
  selector: 'app-status-badge',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<span class="badge" [class]="'badge status-' + status().toLowerCase()">{{
    label()
  }}</span>`,
  styles: `
    .badge {
      display: inline-block;
      padding: 2px 10px;
      border-radius: 12px;
      font-size: 0.8rem;
      background: #e0e0e0;
      color: #212121;
    }
    .status-inprogress {
      background: #bbdefb;
    }
    .status-done {
      background: #c8e6c9;
    }
  `,
})
export class StatusBadge {
  readonly status = input.required<TaskStatus>();
  protected readonly label = computed(() => LABELS[this.status()]);
}
