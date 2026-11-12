import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Button, ButtonVariant } from './button';

@Component({
  imports: [Button],
  template: `<lt-button [variant]="variant()" [size]="size()">Acción</lt-button>`,
})
class Host {
  readonly variant = signal<ButtonVariant>('primary');
  readonly size = signal<'sm' | 'md'>('md');
}

describe('Button: variantes y tamaños', () => {
  const expectedClass: Record<ButtonVariant, string> = {
    primary: 'bg-primary',
    accent: 'bg-secondary-container',
    secondary: 'bg-surface-container-lowest',
    ghost: 'text-on-surface-variant',
    danger: 'bg-error',
  };

  it.each(Object.keys(expectedClass) as ButtonVariant[])(
    'la variante %s usa sus clases de tema',
    (variant) => {
      const fixture = TestBed.createComponent(Host);
      fixture.componentInstance.variant.set(variant);
      fixture.detectChanges();
      const button: HTMLButtonElement = fixture.nativeElement.querySelector('button');

      expect(button.className).toContain(expectedClass[variant]);
    },
  );

  it('todos los tamaños mantienen un área táctil de 44 px', () => {
    const fixture = TestBed.createComponent(Host);
    const button: HTMLButtonElement = fixture.nativeElement.querySelector('button');

    for (const size of ['sm', 'md'] as const) {
      fixture.componentInstance.size.set(size);
      fixture.detectChanges();

      expect(button.className).toContain('min-h-11');
    }
  });
});
