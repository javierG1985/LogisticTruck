import { ChangeDetectionStrategy, Component, computed, HostListener, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NotificationOutlet } from '../../notifications/notification-outlet/notification-outlet';
import { Sidebar } from '../sidebar/sidebar';
import { Topbar } from '../topbar/topbar';

const COLLAPSED_KEY = 'lt-sidebar-collapsed';

// Un valor guardado que no se puede leer (almacenamiento bloqueado) cae a "expandida".
function readCollapsed(): boolean {
  try {
    return localStorage.getItem(COLLAPSED_KEY) === 'true';
  } catch {
    return false;
  }
}

function saveCollapsed(collapsed: boolean): void {
  try {
    localStorage.setItem(COLLAPSED_KEY, String(collapsed));
  } catch {
    // Sin almacenamiento, la preferencia dura hasta recargar la página.
  }
}

// Compone sidebar, topbar y contenido. Escritorio: sidebar fija y contenido desplazado.
// Celular: sidebar como panel que se cierra con Esc o tocando fuera, y devuelve el foco al botón del menú.
@Component({
  selector: 'lt-shell',
  imports: [RouterOutlet, Sidebar, Topbar, NotificationOutlet],
  templateUrl: './shell.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Shell {
  protected readonly collapsed = signal(readCollapsed());
  protected readonly mobileOpen = signal(false);

  protected readonly contentClasses = computed(
    () =>
      `min-w-0 flex-1 transition-[padding] duration-200 motion-reduce:transition-none ${this.collapsed() ? 'lg:pl-18' : 'lg:pl-64'}`,
  );

  protected toggleCollapsed(): void {
    this.collapsed.update((value) => !value);
    saveCollapsed(this.collapsed());
  }

  protected openMobile(): void {
    this.mobileOpen.set(true);
  }

  protected closeMobile(): void {
    if (!this.mobileOpen()) {
      return;
    }

    this.mobileOpen.set(false);
    setTimeout(() => document.querySelector<HTMLElement>('#lt-menu-trigger button')?.focus());
  }

  @HostListener('document:keydown.escape')
  protected onEscape(): void {
    this.closeMobile();
  }
}
