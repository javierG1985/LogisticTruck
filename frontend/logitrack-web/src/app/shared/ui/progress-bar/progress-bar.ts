import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

let nextId = 0;

@Component({
  selector: 'lt-progress-bar',
  templateUrl: './progress-bar.html',
  host: { class: 'block' },
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProgressBar {
  readonly value = input.required<number>();
  readonly max = input(100);
  readonly label = input.required<string>();
  readonly supportText = input<string | null>(null);

  protected readonly labelId = `lt-progress-label-${nextId++}`;
  protected readonly percent = computed(() => {
    const max = this.max();
    const ratio = max > 0 ? this.value() / max : 0;
    return Math.round(Math.min(Math.max(ratio, 0), 1) * 100);
  });
}
