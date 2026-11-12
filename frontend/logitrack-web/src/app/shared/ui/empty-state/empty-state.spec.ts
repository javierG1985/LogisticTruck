import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { EmptyState } from './empty-state';

@Component({
  imports: [EmptyState],
  template: `<lt-empty-state
    icon="assignment_add"
    title="Arrastra órdenes aquí"
    description="Crea la hoja de ruta"
    [dashed]="true"
  >
    <button type="button">Asignar</button>
  </lt-empty-state>`,
})
class Host {}

describe('EmptyState', () => {
  it('muestra título, descripción y la acción proyectada con borde punteado', () => {
    const fixture = TestBed.createComponent(Host);
    fixture.detectChanges();
    const element: HTMLElement = fixture.nativeElement;
    const container: HTMLElement = element.querySelector('lt-empty-state > div')!;

    expect(element.textContent).toContain('Arrastra órdenes aquí');
    expect(element.textContent).toContain('Crea la hoja de ruta');
    expect(element.querySelector('button')?.textContent).toContain('Asignar');
    expect(container.className).toContain('border-dashed');
  });
});
