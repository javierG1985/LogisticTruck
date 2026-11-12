import { ChangeDetectionStrategy, Component } from '@angular/core';
import { Card } from '@shared/ui/card/card';
import { SectionHeader } from '@shared/ui/section-header/section-header';

interface TypeSample {
  readonly name: string;
  readonly classes: string;
  readonly text: string;
}

// Cada clase es completa y literal: Tailwind la encuentra en el código y no se arma en tiempo de ejecución.
const SAMPLES: readonly TypeSample[] = [
  {
    name: 'headline-xl',
    classes: 'font-headline text-headline-xl text-on-surface',
    text: 'Despachos y Planificación',
  },
  {
    name: 'headline-lg',
    classes: 'font-headline text-headline-lg text-on-surface',
    text: 'Torre de Control',
  },
  {
    name: 'headline-md',
    classes: 'font-headline text-headline-md text-on-surface',
    text: 'Hojas de Ruta y Vehículos',
  },
  {
    name: 'headline-sm',
    classes: 'font-headline text-headline-sm text-on-surface',
    text: 'Órdenes por Asignar',
  },
  {
    name: 'body-lg',
    classes: 'text-body-lg text-on-surface',
    text: 'La ruta cubre seis paradas en Bogotá.',
  },
  {
    name: 'body-md',
    classes: 'text-body-md text-on-surface',
    text: 'La ruta cubre seis paradas en Bogotá.',
  },
  {
    name: 'body-sm',
    classes: 'text-body-sm text-on-surface-variant',
    text: 'La ruta cubre seis paradas en Bogotá.',
  },
  { name: 'label-lg', classes: 'text-label-lg text-on-surface', text: 'Prioridad y ventana' },
  { name: 'label-md', classes: 'text-label-md text-on-surface', text: 'Prioridad y ventana' },
  {
    name: 'label-sm',
    classes: 'text-label-sm text-on-surface-variant uppercase',
    text: 'Solo etiquetas secundarias',
  },
  {
    name: 'telemetry-lg',
    classes: 'font-telemetry-lg text-telemetry-lg text-on-surface',
    text: '94,2%',
  },
  {
    name: 'telemetry-md',
    classes: 'font-telemetry-md text-telemetry-md text-on-surface',
    text: '2,4 / 2,8 t',
  },
];

@Component({
  selector: 'lt-type-scale',
  imports: [Card, SectionHeader],
  templateUrl: './type-scale.html',
  host: { class: 'block' },
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TypeScale {
  protected readonly samples = SAMPLES;
}
