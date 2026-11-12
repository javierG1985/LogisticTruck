import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { Icon } from '../icon/icon';

// Botón de solo ícono. El nombre accesible es obligatorio: sin texto visible, label es lo único que anuncia el lector de pantalla.
@Component({
  selector: 'lt-icon-button',
  imports: [Icon],
  templateUrl: './icon-button.html',
  host: { class: 'inline-flex' },
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class IconButton {
  readonly icon = input.required<string>();
  readonly label = input.required<string>();
  readonly disabled = input(false);
}
