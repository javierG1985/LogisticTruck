import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { Sidebar } from './sidebar';

@Component({
  imports: [Sidebar],
  template: `<lt-sidebar
    [collapsed]="collapsed()"
    [mobileOpen]="mobileOpen()"
    (closeRequested)="closes.set(closes() + 1)"
  />`,
})
class Host {
  readonly collapsed = signal(false);
  readonly mobileOpen = signal(false);
  readonly closes = signal(0);
}

function setup(): {
  fixture: ReturnType<typeof TestBed.createComponent<Host>>;
  element: HTMLElement;
} {
  TestBed.configureTestingModule({ providers: [provideRouter([])] });
  const fixture = TestBed.createComponent(Host);
  fixture.detectChanges();
  return { fixture, element: fixture.nativeElement };
}

describe('Sidebar', () => {
  it('convierte el ítem habilitado en un enlace a su ruta', () => {
    const { element } = setup();
    const link = element.querySelector<HTMLAnchorElement>('a');

    expect(link?.getAttribute('href')).toBe('/dashboard');
    expect(link?.textContent).toContain('Torre de Control');
  });

  it('muestra los ítems pendientes como no disponibles con "Próximamente"', () => {
    const { element } = setup();
    const pending = element.querySelectorAll<HTMLElement>('[aria-disabled="true"]');

    expect(pending.length).toBe(7);
    expect(pending[0].textContent).toContain('Próximamente');
    expect(element.querySelectorAll('a[href]').length).toBe(1);
  });

  it('en modo colapsado deja el nombre solo para lectores de pantalla', () => {
    const { fixture, element } = setup();
    fixture.componentInstance.collapsed.set(true);
    fixture.detectChanges();
    const link = element.querySelector<HTMLAnchorElement>('a');

    expect(link?.querySelector('.sr-only')?.textContent).toBe('Torre de Control');
    expect(link?.title).toBe('Torre de Control');
  });

  it('se desliza dentro de la pantalla solo cuando el panel móvil está abierto', () => {
    const { fixture, element } = setup();
    const panel = element.querySelector<HTMLElement>('aside');

    expect(panel?.className).toContain('-translate-x-full');

    fixture.componentInstance.mobileOpen.set(true);
    fixture.detectChanges();

    expect(panel?.className).toContain('translate-x-0');
    expect(panel?.className).not.toContain('-translate-x-full');
  });

  it('pide cerrar el panel desde su botón de cierre', () => {
    const { fixture, element } = setup();
    const close = element.querySelector<HTMLButtonElement>('button[aria-label="Cerrar menú"]');

    close?.click();
    fixture.detectChanges();

    expect(fixture.componentInstance.closes()).toBe(1);
  });
});
