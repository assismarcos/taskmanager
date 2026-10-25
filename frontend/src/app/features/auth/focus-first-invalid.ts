import { AbstractControl } from '@angular/forms';

export function focusFirstInvalid(host: HTMLElement, controls: Record<string, AbstractControl>): void {
  const name = Object.keys(controls).find((key) => controls[key].invalid);
  host.querySelector<HTMLElement>(`[formControlName="${name}"]`)?.focus();
}
