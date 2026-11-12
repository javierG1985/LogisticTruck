import { ChangeDetectionStrategy, Component, input, model } from '@angular/core';

export interface SelectOption {
  readonly value: string;
  readonly label: string;
}

let nextId = 0;

// Etiqueta en línea, como en el diseño, pero asociada al select para que el lector de pantalla la anuncie.
@Component({
  selector: 'lt-select-field',
  templateUrl: './select-field.html',
  host: { class: 'block' },
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SelectField {
  readonly label = input.required<string>();
  readonly options = input.required<readonly SelectOption[]>();
  readonly value = model('');

  protected readonly selectId = `lt-select-${nextId++}`;

  protected onChange(event: Event): void {
    this.value.set((event.target as HTMLSelectElement).value);
  }
}
