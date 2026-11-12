import { ChangeDetectionStrategy, Component, signal } from '@angular/core';
import { Icon } from '../icon/icon';

let nextId = 0;

// Buscador siempre visible. Los filtros se muestran en línea cuando el contenedor es ancho,
// y en celular detrás de un botón que los despliega. Usa consultas de contenedor, no breakpoints de pantalla.
@Component({
  selector: 'lt-filter-bar',
  imports: [Icon],
  templateUrl: './filter-bar.html',
  host: { class: 'block @container' },
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FilterBar {
  readonly expanded = signal(false);

  protected readonly panelId = `lt-filters-${nextId++}`;

  protected toggle(): void {
    this.expanded.update((open) => !open);
  }
}
