import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { PageHeader } from './page-header';

@Component({
  imports: [PageHeader],
  template: `<lt-page-header
    [breadcrumbs]="['Operación', 'Torre de Control']"
    title="Torre de Control"
  >
    <button actions type="button">Exportar</button>
  </lt-page-header>`,
})
class Host {}

describe('PageHeader', () => {
  it('marca la última miga como página actual', () => {
    const fixture = TestBed.createComponent(Host);
    fixture.detectChanges();
    const items: HTMLElement[] = Array.from(fixture.nativeElement.querySelectorAll('nav li'));

    expect(fixture.nativeElement.querySelector('nav')?.getAttribute('aria-label')).toBe(
      'Migas de pan',
    );
    expect(items).toHaveLength(2);
    expect(items[0].getAttribute('aria-current')).toBeNull();
    expect(items[1].getAttribute('aria-current')).toBe('page');
  });

  it('muestra el título como encabezado principal y las acciones proyectadas', () => {
    const fixture = TestBed.createComponent(Host);
    fixture.detectChanges();
    const element: HTMLElement = fixture.nativeElement;

    expect(element.querySelector('h1')?.textContent).toContain('Torre de Control');
    expect(element.querySelector('button[actions]')?.textContent).toContain('Exportar');
  });
});
