import { ChangeDetectionStrategy, Component, input, model } from '@angular/core';

export interface SegmentedOption {
  readonly value: string;
  readonly label: string;
}

let nextId = 0;

// Opciones excluyentes con radios nativos: el teclado (flechas) y el lector de pantalla funcionan sin código extra.
@Component({
  selector: 'lt-segmented-control',
  templateUrl: './segmented-control.html',
  host: { class: 'inline-flex' },
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SegmentedControl {
  readonly label = input.required<string>();
  readonly options = input.required<readonly SegmentedOption[]>();
  readonly value = model('');

  protected readonly groupName = `lt-segmented-${nextId++}`;
}
