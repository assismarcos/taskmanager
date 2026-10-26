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
import { TaskItem } from '../../../core/models/task.model';
import { StatusBadge } from '../../../shared/ui/status-badge';
import { toLocalDate } from '../due-date';

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
    StatusBadge,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <mat-card [class.overdue]="task().isOverdue">
      <mat-card-header>
        <mat-card-title>{{ task().title }}</mat-card-title>
      </mat-card-header>
      <mat-card-content>
        <app-status-badge [status]="task().status" />
        @if (task().isOverdue) {
          <span class="overdue-badge">Overdue</span>
        }
        <p>Due {{ dueDate() | date: 'mediumDate' }}</p>
        @if (task().description) {
          <p>{{ task().description }}</p>
        }
      </mat-card-content>
      <mat-card-actions>
        <a mat-button [routerLink]="['/tasks', task().id, 'edit']">Edit</a>
        <button mat-button type="button" (click)="delete.emit(task())">Delete</button>
      </mat-card-actions>
    </mat-card>
  `,
  styles: `
    .overdue {
      border-left: 4px solid #c62828;
    }
    .overdue-badge {
      margin-left: 8px;
      padding: 2px 10px;
      border-radius: 12px;
      font-size: 0.8rem;
      background: #ffcdd2;
      color: #b71c1c;
    }
  `,
})
export class TaskCard {
  readonly task = input.required<TaskItem>();
  readonly delete = output<TaskItem>();

  protected readonly dueDate = computed(() => toLocalDate(this.task().dueDate));
}
