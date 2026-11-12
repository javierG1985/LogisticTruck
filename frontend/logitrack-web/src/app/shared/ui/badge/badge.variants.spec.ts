import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Badge, BadgeVariant } from './badge';

@Component({
  imports: [Badge],
  template: `<lt-badge [variant]="variant()" [dot]="true" [live]="live()">Estado</lt-badge>`,
})
class Host {
  readonly variant = signal<BadgeVariant>('neutral');
  readonly live = signal(false);
}

describe('Badge: variantes y animación', () => {
  const expectedClass: Record<BadgeVariant, string> = {
    neutral: 'bg-surface-container-high',
    info: 'bg-secondary-fixed',
    success: 'bg-primary-fixed',
    warning: 'bg-tertiary-fixed',
    danger: 'bg-error-container',
  };

  it.each(Object.keys(expectedClass) as BadgeVariant[])(
    'la variante %s usa su fondo de tema',
    (variant) => {
      const fixture = TestBed.createComponent(Host);
      fixture.componentInstance.variant.set(variant);
      fixture.detectChanges();
      const badge: HTMLElement = fixture.nativeElement.querySelector('lt-badge > span');

      expect(badge.className).toContain(expectedClass[variant]);
    },
  );

  it('solo anima el punto cuando el estado está en vivo', () => {
    const fixture = TestBed.createComponent(Host);
    fixture.detectChanges();
    const dot: HTMLElement = fixture.nativeElement.querySelector('lt-badge > span > span');

    expect(dot.className).not.toContain('animate-pulse');

    fixture.componentInstance.live.set(true);
    fixture.detectChanges();

    expect(dot.className).toContain('motion-safe:animate-pulse');
  });
});
