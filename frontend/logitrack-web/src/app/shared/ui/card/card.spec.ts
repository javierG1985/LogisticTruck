import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Card } from './card';

@Component({
  imports: [Card],
  template: `<lt-card padding="lg"><p>Contenido</p></lt-card>`,
})
class Host {}

describe('Card', () => {
  it('proyecta su contenido con el padding elegido', () => {
    const fixture = TestBed.createComponent(Host);
    fixture.detectChanges();
    const card: HTMLElement = fixture.nativeElement.querySelector('lt-card > div');

    expect(card.textContent).toContain('Contenido');
    expect(card.className).toContain('p-space-lg');
  });
});
