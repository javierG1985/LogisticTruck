import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { Icon } from '../icon/icon';

export type ButtonVariant = 'primary' | 'accent' | 'secondary' | 'ghost' | 'danger';
export type ButtonSize = 'sm' | 'md';

const BASE =
  'inline-flex min-h-11 items-center justify-center gap-space-xs rounded-lg transition-all active:scale-95 disabled:pointer-events-none disabled:opacity-50';

const VARIANT_CLASSES: Record<ButtonVariant, string> = {
  primary: 'bg-primary text-on-primary shadow-sm hover:bg-secondary',
  accent: 'bg-secondary-container text-on-secondary-container shadow-sm hover:brightness-110',
  secondary: 'bg-surface-container-lowest text-on-surface shadow-soft hover:bg-surface-container',
  ghost: 'text-on-surface-variant hover:bg-surface-container-high hover:text-on-surface',
  danger: 'bg-error text-on-error shadow-sm hover:brightness-110',
};

const SIZE_CLASSES: Record<ButtonSize, string> = {
  sm: 'px-space-sm font-label-md text-label-md',
  md: 'px-space-md py-space-sm font-label-lg text-label-lg',
};

@Component({
  selector: 'lt-button',
  imports: [Icon],
  templateUrl: './button.html',
  host: { class: 'inline-flex' },
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Button {
  readonly variant = input<ButtonVariant>('primary');
  readonly size = input<ButtonSize>('md');
  readonly type = input<'button' | 'submit'>('button');
  readonly icon = input<string | null>(null);
  readonly loading = input(false);
  readonly disabled = input(false);

  protected readonly classes = computed(
    () => `${BASE} ${VARIANT_CLASSES[this.variant()]} ${SIZE_CLASSES[this.size()]}`,
  );
}
