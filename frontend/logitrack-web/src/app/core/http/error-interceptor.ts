import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { NotificationService } from '../notifications/notification-service';

const NETWORK_MESSAGE = 'Sin conexión con el servidor. Revisa tu red e intenta de nuevo.';
const SERVER_MESSAGE = 'No pudimos completar la operación. Intenta de nuevo en unos minutos.';

// Solo los fallos globales (red caída y errores 5xx) se notifican aquí.
// El resto (validaciones, conflictos, no encontrados) lo maneja la página, que conoce el contexto.
export const errorInterceptor: HttpInterceptorFn = (request, next) => {
  const notifications = inject(NotificationService);

  return next(request).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse) {
        if (error.status === 0) {
          notifications.show(NETWORK_MESSAGE, 'error');
        } else if (error.status >= 500) {
          notifications.show(SERVER_MESSAGE, 'error');
        }
      }

      return throwError(() => error);
    }),
  );
};
