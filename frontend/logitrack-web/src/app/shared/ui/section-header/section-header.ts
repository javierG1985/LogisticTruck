import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { Icon } from '../icon/icon';

export type SectionTone = 'primary' | 'accent';

const TILE_TONE_CLASSES: Record<SectionTone, string> = {
  primary: 'bg-primary-container text-on-primary',
  accent: 'bg-secondary-container text-on-secondary-container',
};

// Encabezado de una sección. Con el espacio justo, la acción baja debajo del título (consulta de contenedor).
@Component({
  selector: 'lt-section-header',
  imports: [Icon],
  templateUrl: './section-header.html',
  host: { class: 'block @container' },
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SectionHeader {
  readonly title = input.required<string>();
  readonly subtitle = input<string | null>(null);
  readonly icon = input<string | null>(null);
  readonly tone = input<SectionTone>('primary');

  protected readonly tileClasses = computed(
    () =>
      `flex h-9 w-9 shrink-0 items-center justify-center rounded-lg ${TILE_TONE_CLASSES[this.tone()]}`,
  );
}
