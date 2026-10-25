import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-tasks-placeholder',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: '<p>Your tasks will appear here.</p>',
})
export class TasksPlaceholderPage {}
