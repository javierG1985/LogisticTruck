import { TestBed } from '@angular/core/testing';
import { ThemeService } from './theme-service';

interface FakeMediaQuery {
  setSystemDark(dark: boolean): void;
}

// Simula la preferencia de color del sistema y permite cambiarla durante la prueba.
function mockSystemTheme(initialDark: boolean): FakeMediaQuery {
  const listeners = new Set<(event: MediaQueryListEvent) => void>();
  const query = {
    matches: initialDark,
    media: '(prefers-color-scheme: dark)',
    addEventListener: (_type: string, listener: (event: MediaQueryListEvent) => void) =>
      listeners.add(listener),
    removeEventListener: (_type: string, listener: (event: MediaQueryListEvent) => void) =>
      listeners.delete(listener),
  };

  vi.stubGlobal('matchMedia', () => query);

  return {
    setSystemDark(dark: boolean) {
      query.matches = dark;
      listeners.forEach((listener) => listener({ matches: dark } as MediaQueryListEvent));
    },
  };
}

function create(): ThemeService {
  const service = TestBed.inject(ThemeService);
  TestBed.flushEffects();
  return service;
}

describe('ThemeService', () => {
  beforeEach(() => {
    localStorage.clear();
  });

  afterEach(() => {
    vi.unstubAllGlobals();
    vi.restoreAllMocks();
    document.documentElement.classList.remove('dark');
    localStorage.clear();
  });

  it('sigue la preferencia del sistema cuando no hay elección guardada', () => {
    mockSystemTheme(true);

    const service = create();

    expect(service.mode()).toBe('system');
    expect(service.isDark()).toBe(true);
    expect(document.documentElement.classList.contains('dark')).toBe(true);
  });

  it('reacciona al cambio de preferencia del sistema en modo sistema', () => {
    const system = mockSystemTheme(false);
    const service = create();

    system.setSystemDark(true);
    TestBed.flushEffects();

    expect(service.isDark()).toBe(true);
    expect(document.documentElement.classList.contains('dark')).toBe(true);
  });

  it('guarda la elección y la aplica aunque el sistema esté en oscuro', () => {
    mockSystemTheme(true);
    const service = create();

    service.set('light');
    TestBed.flushEffects();

    expect(localStorage.getItem('lt-theme')).toBe('light');
    expect(service.isDark()).toBe(false);
    expect(document.documentElement.classList.contains('dark')).toBe(false);
  });

  it('no sigue al sistema cuando el usuario fijó un modo', () => {
    const system = mockSystemTheme(false);
    const service = create();
    service.set('dark');

    system.setSystemDark(false);
    TestBed.flushEffects();

    expect(service.isDark()).toBe(true);
  });

  it('recuerda la elección al crear el servicio otra vez', () => {
    mockSystemTheme(false);
    localStorage.setItem('lt-theme', 'dark');

    const service = create();

    expect(service.mode()).toBe('dark');
  });

  it('ignora un valor guardado que no es un modo válido', () => {
    mockSystemTheme(false);
    localStorage.setItem('lt-theme', 'morado');

    const service = create();

    expect(service.mode()).toBe('system');
  });

  it('no falla si localStorage está bloqueado', () => {
    mockSystemTheme(false);
    vi.spyOn(Storage.prototype, 'setItem').mockImplementation(() => {
      throw new Error('almacenamiento bloqueado');
    });
    const service = create();

    expect(() => service.set('dark')).not.toThrow();
    expect(service.mode()).toBe('dark');
  });

  it('no falla si el navegador no ofrece matchMedia', () => {
    vi.stubGlobal('matchMedia', undefined);

    const service = create();

    expect(service.isDark()).toBe(false);
  });
});
