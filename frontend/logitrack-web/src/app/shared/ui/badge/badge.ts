import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

export type BadgeVariant = 'neutral' | 'info' | 'success' | 'warning' | 'danger';

const BASE =
  'inline-flex items-center gap-1 rounded-full px-space-xs py-0.5 font-label-sm text-label-sm font-semibold';

const VARIANT_CLASSES: Record<BadgeVariant, string> = {
  neutral: 'bg-surface-container-high text-on-surface-variant',
  info: 'bg-secondary-fixed text-on-secondary-fixed-variant',
  success: 'bg-primary-fixed text-on-primary-fixed',
  warning: 'bg-tertiary-fixed text-on-tertiary-fixed',
  danger: 'bg-error-container text-on-error-container',
};

const DOT_CLASSES: Record<BadgeVariant, string> = {
  neutral: 'bg-outline',
  info: 'bg-secondary-container',
  success: 'bg-primary',
  warning: 'bg-tertiary-fixed-dim',
  danger: 'bg-error',
};

// La animación es solo para estados que de verdad están en vivo, y respeta la preferencia de movimiento reducido.
@Component({
  selector: 'lt-badge',
  templateUrl: './badge.html',
  host: { class: 'inline-flex' },
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Badge {
  readonly variant = input<BadgeVariant>('neutral');
  readonly dot = input(false);
  readonly live = input(false);

  protected readonly classes = computed(() => `${BASE} ${VARIANT_CLASSES[this.variant()]}`);
  protected readonly dotClasses = computed(
    () =>
      `h-1.5 w-1.5 rounded-full ${DOT_CLASSES[this.variant()]}${this.live() ? ' motion-safe:animate-pulse' : ''}`,
  );
}
