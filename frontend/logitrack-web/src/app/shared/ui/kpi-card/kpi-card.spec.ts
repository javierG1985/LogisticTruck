import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { KpiCard } from './kpi-card';

@Component({
  imports: [KpiCard],
  template: `<lt-kpi-card
    label="Cubicaje Optimizado"
    [value]="value"
    supportText="+3,8% vs. semana"
    tone="positive"
  />`,
})
class Host {
  value: string | null = null;
}

describe('KpiCard', () => {
  it('muestra "Sin datos" cuando no hay valor', () => {
    const fixture = TestBed.createComponent(Host);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Sin datos');
    expect(fixture.nativeElement.textContent).not.toContain('94');
  });

  it('muestra el valor y el texto de apoyo con su tono', () => {
    const fixture = TestBed.createComponent(Host);
    fixture.componentInstance.value = '94,2%';
    fixture.detectChanges();
    const support: HTMLElement = fixture.nativeElement.querySelector(
      '.font-label-sm.text-secondary',
    );

    expect(fixture.nativeElement.textContent).toContain('94,2%');
    expect(support?.textContent).toContain('+3,8% vs. semana');
  });
});
