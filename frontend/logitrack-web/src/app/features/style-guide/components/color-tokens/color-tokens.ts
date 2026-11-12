import { ChangeDetectionStrategy, Component } from '@angular/core';
import { Card } from '@shared/ui/card/card';
import { SectionHeader } from '@shared/ui/section-header/section-header';

// Todos los tokens de color del tema (variables --lt-*). Se muestran con su nombre, no con su valor.
const COLOR_TOKENS: readonly string[] = [
  'background',
  'on-background',
  'surface',
  'on-surface',
  'surface-bright',
  'surface-dim',
  'surface-variant',
  'on-surface-variant',
  'surface-container-lowest',
  'surface-container-low',
  'surface-container',
  'surface-container-high',
  'surface-container-highest',
  'inverse-surface',
  'inverse-on-surface',
  'outline',
  'outline-variant',
  'primary',
  'on-primary',
  'primary-container',
  'on-primary-container',
  'primary-fixed',
  'primary-fixed-dim',
  'on-primary-fixed',
  'on-primary-fixed-variant',
  'inverse-primary',
  'surface-tint',
  'secondary',
  'on-secondary',
  'secondary-container',
  'on-secondary-container',
  'secondary-fixed',
  'secondary-fixed-dim',
  'on-secondary-fixed',
  'on-secondary-fixed-variant',
  'tertiary',
  'on-tertiary',
  'tertiary-container',
  'on-tertiary-container',
  'tertiary-fixed',
  'tertiary-fixed-dim',
  'on-tertiary-fixed',
  'on-tertiary-fixed-variant',
  'error',
  'on-error',
  'error-container',
  'on-error-container',
];

interface ContrastPair {
  readonly text: string;
  readonly background: string;
  readonly light: number;
  readonly dark: number;
}

// Pares de texto y fondo que se usan juntos en la interfaz. Razón de contraste calculada sobre los valores de styles.css.
const CONTRAST_PAIRS: readonly ContrastPair[] = [
  { text: 'on-surface', background: 'surface', light: 16.23, dark: 14.36 },
  { text: 'on-surface-variant', background: 'surface-container', light: 8.07, dark: 9.65 },
  { text: 'on-surface-variant', background: 'surface-container-lowest', light: 9.39, dark: 11.33 },
  { text: 'on-primary', background: 'primary', light: 21, dark: 7.73 },
  { text: 'on-primary-container', background: 'primary-container', light: 4.56, dark: 9.76 },
  { text: 'on-secondary', background: 'secondary', light: 6.69, dark: 7.74 },
  { text: 'on-secondary-container', background: 'secondary-container', light: 4.55, dark: 7.01 },
  { text: 'on-error', background: 'error', light: 6.46, dark: 7.72 },
  { text: 'on-error-container', background: 'error-container', light: 7.24, dark: 7.24 },
  { text: 'on-tertiary-container', background: 'surface', light: 4.97, dark: 10.89 },
  { text: 'inverse-on-surface', background: 'inverse-surface', light: 11.6, dark: 10.19 },
  { text: 'on-tertiary', background: 'tertiary', light: 21, dark: 13.34 },
  { text: 'on-tertiary-fixed', background: 'tertiary-fixed', light: 13.34, dark: 13.34 },
  { text: 'on-secondary-fixed', background: 'secondary-fixed', light: 13.25, dark: 13.25 },
  { text: 'on-primary-fixed', background: 'primary-fixed', light: 13.3, dark: 13.3 },
  { text: 'on-primary-fixed-variant', background: 'primary-fixed', light: 7.26, dark: 7.26 },
  { text: 'on-tertiary-fixed-variant', background: 'tertiary-fixed', light: 7.26, dark: 7.26 },
];

const AA_TEXT_MINIMUM = 4.5;

@Component({
  selector: 'lt-color-tokens',
  imports: [Card, SectionHeader],
  templateUrl: './color-tokens.html',
  host: { class: 'block' },
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ColorTokens {
  protected readonly tokens = COLOR_TOKENS;
  protected readonly pairs = CONTRAST_PAIRS;
  protected readonly minimum = AA_TEXT_MINIMUM;

  protected swatchColor(name: string): string {
    return `var(--lt-${name})`;
  }

  protected passes(ratio: number): boolean {
    return ratio >= AA_TEXT_MINIMUM;
  }
}
