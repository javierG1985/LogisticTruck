import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { SearchInput } from './search-input';

@Component({
  imports: [SearchInput],
  template: `<lt-search-input label="Buscar camiones" placeholder="Buscar..." [(value)]="term" />`,
})
class Host {
  readonly term = signal('');
}

describe('SearchInput', () => {
  it('asocia el label oculto al campo', () => {
    const fixture = TestBed.createComponent(Host);
    fixture.detectChanges();
    const input: HTMLInputElement = fixture.nativeElement.querySelector('input');
    const label: HTMLLabelElement = fixture.nativeElement.querySelector('label');

    expect(label.getAttribute('for')).toBe(input.id);
    expect(label.textContent).toContain('Buscar camiones');
  });

  it('actualiza el modelo al escribir y limpia con el botón', () => {
    const fixture = TestBed.createComponent(Host);
    fixture.detectChanges();
    const input: HTMLInputElement = fixture.nativeElement.querySelector('input');

    input.value = 'kenworth';
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    expect(fixture.componentInstance.term()).toBe('kenworth');

    const clear: HTMLButtonElement = fixture.nativeElement.querySelector(
      'button[aria-label="Limpiar búsqueda"]',
    );
    clear.click();
    fixture.detectChanges();
    expect(fixture.componentInstance.term()).toBe('');
  });
});
