import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { MatButton } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatSnackBar } from '@angular/material/snack-bar';
import {
  MatCell,
  MatCellDef,
  MatColumnDef,
  MatHeaderCell,
  MatHeaderCellDef,
  MatHeaderRow,
  MatHeaderRowDef,
  MatRow,
  MatRowDef,
  MatTable,
} from '@angular/material/table';
import { RouterLink } from '@angular/router';
import { map } from 'rxjs';
import { TaskItem } from '../../core/models/task.model';
import { ConfirmDialog, ConfirmDialogData } from '../../shared/ui/confirm-dialog';
import { EmptyState } from '../../shared/ui/empty-state';
import { Loading } from '../../shared/ui/loading';
import { StatusBadge } from '../../shared/ui/status-badge';
import { TaskCard } from './components/task-card';
import { TaskFilters } from './components/task-filters';
import { TasksStore } from './data-access/tasks.store';
import { toLocalDate } from './due-date';

@Component({
  selector: 'app-task-list-page',
  imports: [
    DatePipe,
    RouterLink,
    MatButton,
    MatCell,
    MatCellDef,
    MatColumnDef,
    MatHeaderCell,
    MatHeaderCellDef,
    MatHeaderRow,
    MatHeaderRowDef,
    MatPaginator,
    MatRow,
    MatRowDef,
    MatTable,
    EmptyState,
    Loading,
    StatusBadge,
    TaskCard,
    TaskFilters,
  ],
  providers: [TasksStore],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <header class="bar">
      <h1>Tasks</h1>
      <a mat-flat-button routerLink="/tasks/new">New task</a>
    </header>
    <app-task-filters [status]="store.status()" (changed)="store.setStatusFilter($event)" />

    @if (store.loading()) {
      <app-loading />
    } @else if (store.error()) {
      <app-empty-state message="Could not load your tasks.">
        <button mat-button type="button" (click)="store.load()">Retry</button>
      </app-empty-state>
    } @else if (store.tasks().length === 0) {
      <app-empty-state message="No tasks here yet." />
    } @else if (handset()) {
      <div class="cards">
        @for (task of store.tasks(); track task.id) {
          <app-task-card [task]="task" (delete)="confirmDelete($event)" />
        }
      </div>
    } @else {
      <table mat-table [dataSource]="store.tasks()">
        <ng-container matColumnDef="title">
          <th mat-header-cell *matHeaderCellDef>Title</th>
          <td mat-cell *matCellDef="let task">{{ task.title }}</td>
        </ng-container>
        <ng-container matColumnDef="status">
          <th mat-header-cell *matHeaderCellDef>Status</th>
          <td mat-cell *matCellDef="let task">
            <app-status-badge [status]="task.status" />
            @if (task.isOverdue) {
              <span class="overdue-badge">Overdue</span>
            }
          </td>
        </ng-container>
        <ng-container matColumnDef="dueDate">
          <th mat-header-cell *matHeaderCellDef>Due</th>
          <td mat-cell *matCellDef="let task">
            {{ localDate(task.dueDate) | date: 'mediumDate' }}
          </td>
        </ng-container>
        <ng-container matColumnDef="actions">
          <th mat-header-cell *matHeaderCellDef></th>
          <td mat-cell *matCellDef="let task">
            <a mat-button [routerLink]="['/tasks', task.id, 'edit']">Edit</a>
            <button mat-button type="button" (click)="confirmDelete(task)">Delete</button>
          </td>
        </ng-container>
        <tr mat-header-row *matHeaderRowDef="columns"></tr>
        <tr mat-row *matRowDef="let row; columns: columns" [class.overdue]="row.isOverdue"></tr>
      </table>
    }

    <mat-paginator
      [length]="store.totalCount()"
      [pageIndex]="store.page() - 1"
      [pageSize]="store.pageSize()"
      [pageSizeOptions]="[10, 20, 50]"
      (page)="onPage($event)"
    />
  `,
  styles: `
    .bar {
      display: flex;
      justify-content: space-between;
      align-items: center;
    }
    table {
      width: 100%;
    }
    .cards {
      display: grid;
      gap: 12px;
    }
    .overdue {
      background: #fff3f3;
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
export class TaskListPage implements OnInit {
  protected readonly store = inject(TasksStore);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly breakpoints = inject(BreakpointObserver);

  protected readonly columns = ['title', 'status', 'dueDate', 'actions'];
  protected readonly handset = toSignal(
    this.breakpoints.observe(Breakpoints.Handset).pipe(map((state) => state.matches)),
    { initialValue: false },
  );
  protected readonly localDate = toLocalDate;

  ngOnInit(): void {
    this.store.load();
  }

  protected onPage(event: PageEvent): void {
    this.store.setPage(event.pageIndex + 1, event.pageSize);
  }

  protected confirmDelete(task: TaskItem): void {
    const data: ConfirmDialogData = {
      title: 'Delete task',
      message: `Delete "${task.title}"? This cannot be undone.`,
      confirmLabel: 'Delete',
    };
    this.dialog
      .open<ConfirmDialog, ConfirmDialogData, boolean>(ConfirmDialog, { data })
      .afterClosed()
      .subscribe((confirmed) => {
        if (confirmed) {
          this.store.remove(task.id).subscribe({
            next: () => this.snackBar.open('Task deleted', 'OK', { duration: 3000 }),
            error: () => this.snackBar.open('Could not delete the task', 'OK', { duration: 5000 }),
          });
        }
      });
  }
}
