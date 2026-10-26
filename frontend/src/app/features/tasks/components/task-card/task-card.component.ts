import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { MatButton } from '@angular/material/button';
import {
  MatCard,
  MatCardActions,
  MatCardContent,
  MatCardHeader,
  MatCardTitle,
} from '@angular/material/card';
import { RouterLink } from '@angular/router';
import { TaskItem } from '../../../../core/models/task.model';
import { StatusBadgeComponent } from '../../../../shared/ui/status-badge/status-badge.component';
import { toLocalDate } from '../../due-date';

@Component({
  selector: 'app-task-card',
  imports: [
    DatePipe,
    RouterLink,
    MatButton,
    MatCard,
    MatCardActions,
    MatCardContent,
    MatCardHeader,
    MatCardTitle,
    StatusBadgeComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './task-card.component.html',
  styleUrl: './task-card.component.scss',
})
export class TaskCardComponent {
  readonly task = input.required<TaskItem>();
  readonly delete = output<TaskItem>();

  protected readonly dueDate = computed(() => toLocalDate(this.task().dueDate));
}
