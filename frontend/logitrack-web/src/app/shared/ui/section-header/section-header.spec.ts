import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { SectionHeader } from './section-header';

@Component({
  imports: [SectionHeader],
  template: `<lt-section-header
    title="Órdenes por Asignar"
    subtitle="18 órdenes activas"
    icon="inventory_2"
  >
    <button actions type="button">Filtrar</button>
  </lt-section-header>`,
})
class Host {}

describe('SectionHeader', () => {
  it('muestra título, subtítulo y la acción proyectada', () => {
    const fixture = TestBed.createComponent(Host);
    fixture.detectChanges();
    const element: HTMLElement = fixture.nativeElement;

    expect(element.querySelector('h2')?.textContent).toContain('Órdenes por Asignar');
    expect(element.textContent).toContain('18 órdenes activas');
    expect(element.querySelector('button[actions]')?.textContent).toContain('Filtrar');
  });
});
