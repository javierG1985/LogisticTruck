import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Button } from './button';

@Component({
  imports: [Button],
  template: `<lt-button [loading]="loading()" variant="accent">Nuevo Despacho</lt-button>`,
})
class Host {
  readonly loading = signal(false);
}

describe('Button', () => {
  it('muestra el texto proyectado con su variante', () => {
    const fixture = TestBed.createComponent(Host);
    fixture.detectChanges();
    const button: HTMLButtonElement = fixture.nativeElement.querySelector('button');

    expect(button.textContent).toContain('Nuevo Despacho');
    expect(button.className).toContain('bg-secondary-container');
    expect(button.disabled).toBe(false);
  });

  it('se deshabilita y se marca como ocupado mientras carga', () => {
    const fixture = TestBed.createComponent(Host);
    fixture.componentInstance.loading.set(true);
    fixture.detectChanges();
    const button: HTMLButtonElement = fixture.nativeElement.querySelector('button');

    expect(button.disabled).toBe(true);
    expect(button.getAttribute('aria-busy')).toBe('true');
  });
});
