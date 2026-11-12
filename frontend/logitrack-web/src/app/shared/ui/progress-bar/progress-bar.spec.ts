import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { ProgressBar } from './progress-bar';

@Component({
  imports: [ProgressBar],
  template: `<lt-progress-bar label="Capacidad Ocupada" [value]="value" [max]="max" />`,
})
class Host {
  value = 85;
  max = 100;
}

describe('ProgressBar', () => {
  it('expone los atributos aria del progreso y el porcentaje', () => {
    const fixture = TestBed.createComponent(Host);
    fixture.detectChanges();
    const bar: HTMLElement = fixture.nativeElement.querySelector('[role="progressbar"]');

    expect(bar.getAttribute('aria-valuenow')).toBe('85');
    expect(bar.getAttribute('aria-valuemax')).toBe('100');
    expect(bar.getAttribute('aria-labelledby')).toBeTruthy();
    expect(fixture.nativeElement.textContent).toContain('85%');
  });

  it('acota el porcentaje entre 0 y 100', () => {
    const fixture = TestBed.createComponent(Host);
    fixture.componentInstance.value = 250;
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('100%');
  });
});
