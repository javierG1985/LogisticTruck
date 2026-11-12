import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Badge } from './badge';

@Component({
  imports: [Badge],
  template: `<lt-badge variant="danger" [dot]="true" [live]="true">Prioridad Alta</lt-badge>`,
})
class Host {}

describe('Badge', () => {
  it('muestra el texto con la variante y un punto decorativo', () => {
    const fixture = TestBed.createComponent(Host);
    fixture.detectChanges();
    const badge: HTMLElement = fixture.nativeElement.querySelector('lt-badge span');
    const dot: HTMLElement = badge.querySelector('span')!;

    expect(badge.textContent).toContain('Prioridad Alta');
    expect(badge.className).toContain('bg-error-container');
    expect(dot.getAttribute('aria-hidden')).toBe('true');
    expect(dot.className).toContain('motion-safe:animate-pulse');
  });
});
