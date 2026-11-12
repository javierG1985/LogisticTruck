import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

const PADDING_CLASSES = {
  md: 'p-space-md',
  lg: 'p-space-lg',
} as const;

@Component({
  selector: 'lt-card',
  templateUrl: './card.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Card {
  readonly padding = input<keyof typeof PADDING_CLASSES>('md');

  protected readonly classes = computed(
    () => `rounded-xl bg-surface-container-lowest shadow-soft ${PADDING_CLASSES[this.padding()]}`,
  );
}
