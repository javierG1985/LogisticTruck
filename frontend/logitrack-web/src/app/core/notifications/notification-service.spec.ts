import { TestBed } from '@angular/core/testing';
import { NotificationService } from './notification-service';

describe('NotificationService', () => {
  let service: NotificationService;

  beforeEach(() => {
    vi.useFakeTimers();
    service = TestBed.inject(NotificationService);
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it('agrega avisos con su tipo y su mensaje', () => {
    service.show('Guardado', 'success');
    service.show('Algo falló', 'error');

    expect(service.notifications().map((item) => [item.kind, item.message])).toEqual([
      ['success', 'Guardado'],
      ['error', 'Algo falló'],
    ]);
  });

  it('quita un aviso cuando se cierra manualmente', () => {
    service.show('Uno');
    const [first] = service.notifications();

    service.dismiss(first.id);

    expect(service.notifications()).toHaveLength(0);
  });

  it('quita el aviso solo después de seis segundos', () => {
    service.show('Temporal');

    vi.advanceTimersByTime(5999);
    expect(service.notifications()).toHaveLength(1);

    vi.advanceTimersByTime(1);
    expect(service.notifications()).toHaveLength(0);
  });
});
