import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { IconButton } from '@shared/ui/icon-button/icon-button';
import { Icon } from '@shared/ui/icon/icon';
import { NotificationKind, NotificationService } from '../notification-service';

const KIND_ICONS: Record<NotificationKind, string> = {
  success: 'check_circle',
  error: 'error',
  info: 'info',
};

const KIND_CLASSES: Record<NotificationKind, string> = {
  success: 'border-l-primary text-on-surface',
  error: 'border-l-error text-on-surface',
  info: 'border-l-secondary text-on-surface',
};

// La región siempre existe en el DOM para que el lector de pantalla anuncie los avisos que aparecen dentro de ella.
@Component({
  selector: 'lt-notification-outlet',
  imports: [Icon, IconButton],
  templateUrl: './notification-outlet.html',
  host: { class: 'block' },
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NotificationOutlet {
  private readonly service = inject(NotificationService);

  protected readonly items = computed(() =>
    this.service.notifications().map((notification) => ({
      ...notification,
      icon: KIND_ICONS[notification.kind],
      classes: `flex items-start gap-space-sm rounded-xl border-l-4 bg-surface-container-lowest p-space-md shadow-soft-md ${KIND_CLASSES[notification.kind]}`,
    })),
  );

  protected dismiss(id: number): void {
    this.service.dismiss(id);
  }
}
