import { ErrorHandler, inject, Injectable } from '@angular/core';
import { NotificationService } from '../notifications/notification-service';

// Lo no controlado: se registra en consola para diagnóstico, pero el usuario ve un aviso sin detalles técnicos.
@Injectable()
export class GlobalErrorHandler implements ErrorHandler {
  private readonly notifications = inject(NotificationService);

  handleError(error: unknown): void {
    console.error(error);
    this.notifications.show(
      'Ocurrió un error inesperado. Recarga la página si el problema continúa.',
      'error',
    );
  }
}
