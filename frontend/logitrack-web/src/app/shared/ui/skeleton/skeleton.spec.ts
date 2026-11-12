import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Skeleton } from './skeleton';

@Component({
  imports: [Skeleton],
  template: `<lt-skeleton [lines]="4" label="Cargando camiones" />`,
})
class Host {}

describe('Skeleton', () => {
  it('se anuncia como estado ocupado y dibuja una barra por línea', () => {
    const fixture = TestBed.createComponent(Host);
    fixture.detectChanges();
    const status: HTMLElement = fixture.nativeElement.querySelector('[role="status"]');
    const bars = fixture.nativeElement.querySelectorAll('[role="status"] > div');

    expect(status.getAttribute('aria-busy')).toBe('true');
    expect(status.textContent).toContain('Cargando camiones');
    expect(bars).toHaveLength(4);
  });
});
