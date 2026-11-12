import { DOCUMENT } from '@angular/common';
import { computed, DestroyRef, effect, inject, Injectable, signal } from '@angular/core';

export type ThemeMode = 'light' | 'dark' | 'system';

const STORAGE_KEY = 'lt-theme';
const DARK_QUERY = '(prefers-color-scheme: dark)';

// Guarda la elección del usuario. localStorage puede no estar disponible (modo privado, datos bloqueados),
// así que cualquier fallo cae al modo "sistema" sin romper la app.
function readStoredMode(): ThemeMode {
  try {
    const stored = localStorage.getItem(STORAGE_KEY);
    return stored === 'light' || stored === 'dark' ? stored : 'system';
  } catch {
    return 'system';
  }
}

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly document = inject(DOCUMENT);
  // matchMedia puede no existir (entornos sin navegador completo); en ese caso se asume modo claro.
  private readonly media = this.document.defaultView?.matchMedia?.(DARK_QUERY) ?? null;

  readonly mode = signal<ThemeMode>(readStoredMode());
  private readonly systemPrefersDark = signal(this.media?.matches ?? false);

  readonly isDark = computed(() => {
    const mode = this.mode();
    return mode === 'dark' || (mode === 'system' && this.systemPrefersDark());
  });

  constructor() {
    const onSystemChange = (event: MediaQueryListEvent) =>
      this.systemPrefersDark.set(event.matches);
    this.media?.addEventListener('change', onSystemChange);
    inject(DestroyRef).onDestroy(() => this.media?.removeEventListener('change', onSystemChange));

    effect(() => {
      this.document.documentElement.classList.toggle('dark', this.isDark());
    });
  }

  set(mode: ThemeMode): void {
    this.mode.set(mode);

    try {
      localStorage.setItem(STORAGE_KEY, mode);
    } catch {
      // Sin almacenamiento, la elección dura hasta recargar la página.
    }
  }
}
