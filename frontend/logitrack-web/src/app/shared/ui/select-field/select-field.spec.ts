import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { SelectField } from './select-field';

@Component({
  imports: [SelectField],
  template: `<lt-select-field label="Hub:" [options]="options" [(value)]="hub" />`,
})
class Host {
  readonly options = [
    { value: 'san-bernardo', label: 'San Bernardo (Central)' },
    { value: 'pudahuel', label: 'Pudahuel Norte' },
  ];
  readonly hub = signal('pudahuel');
}

describe('SelectField', () => {
  it('asocia la etiqueta al select y marca la opción del modelo', () => {
    const fixture = TestBed.createComponent(Host);
    fixture.detectChanges();
    const select: HTMLSelectElement = fixture.nativeElement.querySelector('select');
    const label: HTMLLabelElement = fixture.nativeElement.querySelector('label');

    expect(label.getAttribute('for')).toBe(select.id);
    expect(select.value).toBe('pudahuel');
  });

  it('actualiza el modelo al elegir otra opción', () => {
    const fixture = TestBed.createComponent(Host);
    fixture.detectChanges();
    const select: HTMLSelectElement = fixture.nativeElement.querySelector('select');

    select.value = 'san-bernardo';
    select.dispatchEvent(new Event('change'));
    fixture.detectChanges();

    expect(fixture.componentInstance.hub()).toBe('san-bernardo');
  });
});
