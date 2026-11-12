import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  HostListener,
  inject,
  signal,
} from '@angular/core';
import { Icon } from '@shared/ui/icon/icon';

let nextId = 0;

// Sin autenticación todavía: muestra un usuario de demostración y sus acciones quedan "Próximamente".
@Component({
  selector: 'lt-user-menu',
  imports: [Icon],
  templateUrl: './user-menu.html',
  host: { class: 'relative block' },
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserMenu {
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);

  protected readonly open = signal(false);
  protected readonly menuId = `lt-user-menu-${nextId++}`;

  protected toggle(): void {
    this.open.update((value) => !value);
  }

  @HostListener('document:click', ['$event.target'])
  protected onDocumentClick(target: EventTarget | null): void {
    if (this.open() && target instanceof Node && !this.host.nativeElement.contains(target)) {
      this.open.set(false);
    }
  }

  @HostListener('document:keydown.escape')
  protected onEscape(): void {
    this.open.set(false);
  }
}
