import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { Icon } from '../icon/icon';

export type KpiTone = 'positive' | 'neutral' | 'negative';

const SUPPORT_TONE_CLASSES: Record<KpiTone, string> = {
  positive: 'text-secondary font-semibold',
  neutral: 'text-on-surface-variant',
  negative: 'text-error font-semibold',
};

// Sin valor, muestra un guion y lo anuncia como "Sin datos": nunca se inventan cifras.
@Component({
  selector: 'lt-kpi-card',
  imports: [Icon],
  templateUrl: './kpi-card.html',
  host: { class: 'block' },
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class KpiCard {
  readonly label = input.required<string>();
  readonly value = input<string | null>(null);
  readonly supportText = input<string | null>(null);
  readonly tone = input<KpiTone>('neutral');
  readonly icon = input<string | null>(null);

  protected readonly supportClasses = computed(
    () => `mt-0.5 block font-label-sm text-label-sm ${SUPPORT_TONE_CLASSES[this.tone()]}`,
  );
}
