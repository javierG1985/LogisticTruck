import { ChangeDetectionStrategy, Component } from '@angular/core';
import { Card } from '@shared/ui/card/card';
import { SectionHeader } from '@shared/ui/section-header/section-header';

interface SpaceSample {
  readonly name: string;
  readonly classes: string;
}

interface ShapeSample {
  readonly name: string;
  readonly classes: string;
}

// Las clases van completas en el código para que Tailwind las genere. "rounded-full" conserva su significado de círculo.
const SPACING: readonly SpaceSample[] = [
  { name: 'space-xs', classes: 'h-space-xs w-space-xs bg-primary' },
  { name: 'space-sm', classes: 'h-space-sm w-space-sm bg-primary' },
  { name: 'space-md', classes: 'h-space-md w-space-md bg-primary' },
  { name: 'space-lg', classes: 'h-space-lg w-space-lg bg-primary' },
  { name: 'space-xl', classes: 'h-space-xl w-space-xl bg-primary' },
  { name: 'gutter', classes: 'h-gutter w-gutter bg-secondary' },
  { name: 'gutter-desktop', classes: 'h-gutter-desktop w-gutter-desktop bg-secondary' },
  { name: 'margin', classes: 'h-margin w-margin bg-secondary' },
  { name: 'margin-desktop', classes: 'h-margin-desktop w-margin-desktop bg-secondary' },
];

const SHAPES: readonly ShapeSample[] = [
  { name: 'rounded-sm', classes: 'h-16 w-16 rounded-sm bg-surface-container-high' },
  { name: 'rounded-lg', classes: 'h-16 w-16 rounded-lg bg-surface-container-high' },
  { name: 'rounded-xl', classes: 'h-16 w-16 rounded-xl bg-surface-container-high' },
  { name: 'rounded-full', classes: 'h-16 w-16 rounded-full bg-surface-container-high' },
];

const SHADOWS: readonly ShapeSample[] = [
  {
    name: 'shadow-soft',
    classes: 'h-16 w-full rounded-xl bg-surface-container-lowest shadow-soft',
  },
  {
    name: 'shadow-soft-md',
    classes: 'h-16 w-full rounded-xl bg-surface-container-lowest shadow-soft-md',
  },
];

@Component({
  selector: 'lt-shape-and-space',
  imports: [Card, SectionHeader],
  templateUrl: './shape-and-space.html',
  host: { class: 'block' },
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ShapeAndSpace {
  protected readonly spacing = SPACING;
  protected readonly shapes = SHAPES;
  protected readonly shadows = SHADOWS;
}
