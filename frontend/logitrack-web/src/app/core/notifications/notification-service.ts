import { Injectable, signal } from '@angular/core';

export type NotificationKind = 'success' | 'error' | 'info';

export interface AppNotification {
  readonly id: number;
  readonly kind: NotificationKind;
  readonly message: string;
}

const AUTO_DISMISS_MS = 6000;

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private nextId = 0;

  readonly notifications = signal<readonly AppNotification[]>([]);

  show(message: string, kind: NotificationKind = 'info'): void {
    const id = ++this.nextId;
    this.notifications.update((list) => [...list, { id, kind, message }]);
    setTimeout(() => this.dismiss(id), AUTO_DISMISS_MS);
  }

  dismiss(id: number): void {
    this.notifications.update((list) => list.filter((notification) => notification.id !== id));
  }
}
