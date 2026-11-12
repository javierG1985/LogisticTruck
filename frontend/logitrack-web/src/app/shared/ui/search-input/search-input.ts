import { ChangeDetectionStrategy, Component, input, model } from '@angular/core';
import { Icon } from '../icon/icon';
import { IconButton } from '../icon-button/icon-button';

let nextId = 0;

// El label es obligatorio aunque esté oculto: un buscador sin nombre accesible no se entiende con lector de pantalla.
// El texto de los campos es de 16 px en celular para evitar el zoom automático de iOS.
@Component({
  selector: 'lt-search-input',
  imports: [Icon, IconButton],
  templateUrl: './search-input.html',
  host: { class: 'block' },
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SearchInput {
  readonly label = input.required<string>();
  readonly placeholder = input('');
  readonly disabled = input(false);
  readonly value = model('');

  protected readonly inputId = `lt-search-${nextId++}`;

  protected onInput(event: Event): void {
    this.value.set((event.target as HTMLInputElement).value);
  }

  protected clear(): void {
    this.value.set('');
  }
}
