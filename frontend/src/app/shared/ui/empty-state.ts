import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'app-empty-state',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<div class="empty">
    <p>{{ message() }}</p>
    <ng-content />
  </div>`,
  styles: `
    .empty {
      text-align: center;
      padding: 32px;
      color: #616161;
    }
  `,
})
export class EmptyState {
  readonly message = input.required<string>();
}
