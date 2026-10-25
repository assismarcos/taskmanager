import { ChangeDetectionStrategy, Component } from '@angular/core';
import { MatProgressSpinner } from '@angular/material/progress-spinner';

@Component({
  selector: 'app-loading',
  imports: [MatProgressSpinner],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<div class="loading" role="status" aria-label="Loading">
    <mat-spinner diameter="40" />
  </div>`,
  styles: `
    .loading {
      display: flex;
      justify-content: center;
      padding: 32px;
    }
  `,
})
export class Loading {}
