import { CdkTrapFocus } from '@angular/cdk/a11y';
import { TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';
import { Shell } from './shell';

function setup(): {
  element: HTMLElement;
  detectChanges: () => void;
  trap: () => CdkTrapFocus;
} {
  TestBed.configureTestingModule({ imports: [Shell], providers: [provideRouter([])] });
  const fixture = TestBed.createComponent(Shell);
  fixture.detectChanges();
  return {
    element: fixture.nativeElement,
    detectChanges: () => fixture.detectChanges(),
    trap: () => fixture.debugElement.query(By.directive(CdkTrapFocus)).injector.get(CdkTrapFocus),
  };
}

function openMenu(element: HTMLElement): void {
  element.querySelector<HTMLButtonElement>('#lt-menu-trigger button')?.click();
}

function panelOf(element: HTMLElement): HTMLElement {
  return element.querySelector<HTMLElement>('aside')!;
}

describe('Shell', () => {
  beforeEach(() => {
    vi.stubGlobal('matchMedia', () => ({
      matches: false,
      addEventListener: () => undefined,
      removeEventListener: () => undefined,
    }));
    localStorage.clear();
  });

  afterEach(() => {
    vi.unstubAllGlobals();
    localStorage.clear();
    document.querySelectorAll('.cdk-overlay-container').forEach((element) => element.remove());
  });

  it('ofrece un enlace para saltar al contenido', () => {
    const { element } = setup();
    const skip = element.querySelector<HTMLAnchorElement>('a[href="#contenido"]');

    expect(skip?.textContent).toContain('Saltar al contenido');
    expect(element.querySelector('main#contenido')).not.toBeNull();
  });

  it('empieza con el panel móvil cerrado', () => {
    const { element } = setup();

    expect(panelOf(element).className).toContain('-translate-x-full');
    expect(element.querySelector('button[aria-label="Cerrar menú"][class*="fixed"]')).toBeNull();
  });

  it('abre el panel desde el botón del menú y muestra el fondo para cerrarlo', () => {
    const { element, detectChanges } = setup();

    openMenu(element);
    detectChanges();

    expect(panelOf(element).className).toContain('translate-x-0');
    expect(element.querySelector('button.fixed[aria-label="Cerrar menú"]')).not.toBeNull();
  });

  it('se cierra con la tecla Escape', () => {
    const { element, detectChanges } = setup();
    openMenu(element);
    detectChanges();

    document.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape' }));
    detectChanges();

    expect(panelOf(element).className).toContain('-translate-x-full');
  });

  it('se cierra al tocar fuera del panel', () => {
    const { element, detectChanges } = setup();
    openMenu(element);
    detectChanges();

    element.querySelector<HTMLButtonElement>('button.fixed[aria-label="Cerrar menú"]')?.click();
    detectChanges();

    expect(panelOf(element).className).toContain('-translate-x-full');
  });

  it('mantiene el foco dentro del panel mientras está abierto', () => {
    const { element, detectChanges, trap } = setup();
    expect(trap().enabled).toBe(false);

    openMenu(element);
    detectChanges();

    expect(trap().enabled).toBe(true);
  });
});
