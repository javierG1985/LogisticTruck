import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { FilterBar } from './filter-bar';

@Component({
  imports: [FilterBar],
  template: `<lt-filter-bar>
    <input search type="text" aria-label="Buscar" />
    <select filters aria-label="Estado">
      <option>Ver todos</option>
    </select>
  </lt-filter-bar>`,
})
class Host {}

describe('FilterBar', () => {
  it('oculta los filtros en celular hasta que se despliegan', () => {
    const fixture = TestBed.createComponent(Host);
    fixture.detectChanges();
    const toggle: HTMLButtonElement = fixture.nativeElement.querySelector('button');
    const panel: HTMLElement = fixture.nativeElement.querySelector(
      `#${toggle.getAttribute('aria-controls')}`,
    );

    expect(toggle.getAttribute('aria-expanded')).toBe('false');
    expect(panel.classList).toContain('hidden');

    toggle.click();
    fixture.detectChanges();

    expect(toggle.getAttribute('aria-expanded')).toBe('true');
    expect(panel.classList).not.toContain('hidden');
  });

  it('proyecta el buscador siempre visible', () => {
    const fixture = TestBed.createComponent(Host);
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('input[search]')).not.toBeNull();
  });
});
