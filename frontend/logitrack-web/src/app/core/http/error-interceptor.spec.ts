import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { NotificationService } from '../notifications/notification-service';
import { errorInterceptor } from './error-interceptor';

describe('errorInterceptor', () => {
  let http: HttpClient;
  let controller: HttpTestingController;
  let notifications: NotificationService;

  beforeEach(() => {
    vi.useFakeTimers();
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([errorInterceptor])),
        provideHttpClientTesting(),
      ],
    });
    http = TestBed.inject(HttpClient);
    controller = TestBed.inject(HttpTestingController);
    notifications = TestBed.inject(NotificationService);
  });

  afterEach(() => {
    controller.verify();
    vi.useRealTimers();
  });

  it('notifica un error 500 con un mensaje genérico', () => {
    http.get('/api/trucks').subscribe({ error: () => undefined });

    controller.expectOne('/api/trucks').flush('boom', { status: 500, statusText: 'Server Error' });

    expect(notifications.notifications()).toHaveLength(1);
    expect(notifications.notifications()[0].kind).toBe('error');
    expect(notifications.notifications()[0].message).toContain('No pudimos completar la operación');
  });

  it('notifica la red caída con un mensaje propio', () => {
    http.get('/api/trucks').subscribe({ error: () => undefined });

    controller.expectOne('/api/trucks').error(new ProgressEvent('error'));

    expect(notifications.notifications()[0].message).toContain('Sin conexión');
  });

  it('no notifica un 404 y deja el error para la página', () => {
    const received: number[] = [];
    http.get('/api/trucks/1').subscribe({ error: (error) => received.push(error.status) });

    controller
      .expectOne('/api/trucks/1')
      .flush({ detail: 'No existe' }, { status: 404, statusText: 'Not Found' });

    expect(notifications.notifications()).toHaveLength(0);
    expect(received).toEqual([404]);
  });

  it('no notifica un 409 de conflicto', () => {
    http.post('/api/trucks', {}).subscribe({ error: () => undefined });

    controller
      .expectOne('/api/trucks')
      .flush({ detail: 'Ya existe' }, { status: 409, statusText: 'Conflict' });

    expect(notifications.notifications()).toHaveLength(0);
  });
});
