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
import { TaskItem } from '../../../core/models/task.model';
import {
  ConfirmDialogComponent,
  ConfirmDialogData,
} from '../../../shared/ui/confirm-dialog/confirm-dialog.component';
import { EmptyStateComponent } from '../../../shared/ui/empty-state/empty-state.component';
import { LoadingComponent } from '../../../shared/ui/loading/loading.component';
import { StatusBadgeComponent } from '../../../shared/ui/status-badge/status-badge.component';
import { TaskCardComponent } from '../components/task-card/task-card.component';
import { TaskFiltersComponent } from '../components/task-filters/task-filters.component';
import { TasksStore } from '../data-access/tasks.store';
import { toLocalDate } from '../due-date';

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
    EmptyStateComponent,
    LoadingComponent,
    StatusBadgeComponent,
    TaskCardComponent,
    TaskFiltersComponent,
  ],
  providers: [TasksStore],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './task-list.component.html',
  styleUrl: './task-list.component.scss',
})
export class TaskListComponent implements OnInit {
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
      .open<ConfirmDialogComponent, ConfirmDialogData, boolean>(ConfirmDialogComponent, { data })
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
