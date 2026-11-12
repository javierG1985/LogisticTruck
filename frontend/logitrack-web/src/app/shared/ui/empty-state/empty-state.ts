import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { Icon } from '../icon/icon';

const BASE =
  'flex flex-col items-center justify-center gap-space-sm rounded-xl p-space-lg text-center';
const SOLID = 'bg-surface-container-lowest shadow-soft';
const DASHED = 'border-2 border-dashed border-outline-variant bg-surface-container-low';

// Estado vacío con una acción opcional (contenido proyectado). La variante punteada indica una zona donde soltar algo.
@Component({
  selector: 'lt-empty-state',
  imports: [Icon],
  templateUrl: './empty-state.html',
  host: { class: 'block' },
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EmptyState {
  readonly icon = input.required<string>();
  readonly title = input.required<string>();
  readonly description = input<string | null>(null);
  readonly dashed = input(false);

  protected readonly classes = computed(() => `${BASE} ${this.dashed() ? DASHED : SOLID}`);
}
