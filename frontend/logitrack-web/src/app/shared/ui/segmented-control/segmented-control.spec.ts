import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { SegmentedControl } from './segmented-control';

@Component({
  imports: [SegmentedControl],
  template: `<lt-segmented-control
    label="Vista del despacho"
    [options]="options"
    [(value)]="view"
  />`,
})
class Host {
  readonly options = [
    { value: 'cards', label: 'Vista Tarjetas' },
    { value: 'matrix', label: 'Matriz Horaria' },
  ];
  readonly view = signal('cards');
}

describe('SegmentedControl', () => {
  it('es un grupo de radios con nombre accesible', () => {
    const fixture = TestBed.createComponent(Host);
    fixture.detectChanges();
    const group: HTMLElement = fixture.nativeElement.querySelector('[role="radiogroup"]');
    const radios: HTMLInputElement[] = Array.from(
      fixture.nativeElement.querySelectorAll('input[type="radio"]'),
    );

    expect(group.getAttribute('aria-label')).toBe('Vista del despacho');
    expect(radios).toHaveLength(2);
    expect(radios[0].checked).toBe(true);
    expect(new Set(radios.map((radio) => radio.name)).size).toBe(1);
  });

  it('actualiza el modelo al elegir otra opción', () => {
    const fixture = TestBed.createComponent(Host);
    fixture.detectChanges();
    const matrix: HTMLInputElement = fixture.nativeElement.querySelector('input[value="matrix"]');

    matrix.click();
    fixture.detectChanges();

    expect(fixture.componentInstance.view()).toBe('matrix');
  });
});
