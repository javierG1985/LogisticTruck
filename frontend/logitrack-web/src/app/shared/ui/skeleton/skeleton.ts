import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

// Marcador de carga. Se anuncia como estado ocupado; las barras no tienen movimiento si el usuario lo pide.
@Component({
  selector: 'lt-skeleton',
  templateUrl: './skeleton.html',
  host: { class: 'block' },
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Skeleton {
  readonly lines = input(3);
  readonly label = input('Cargando');

  protected readonly rows = computed(() =>
    Array.from({ length: this.lines() }, (_, index) => index),
  );
}
