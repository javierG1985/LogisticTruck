import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { ConfirmDialog } from './confirm-dialog';

@Component({
  imports: [ConfirmDialog],
  template: `<lt-confirm-dialog
    title="Eliminar camión"
    message="Esta acción da de baja el camión."
    confirmLabel="Eliminar"
    confirmVariant="danger"
    [(open)]="open"
    (confirmed)="confirmed.set(confirmed() + 1)"
  />`,
})
class Host {
  readonly open = signal(false);
  readonly confirmed = signal(0);
}

describe('ConfirmDialog', () => {
  afterEach(() => {
    document.querySelectorAll('.cdk-overlay-container').forEach((element) => element.remove());
  });

  it('se abre como diálogo modal con su título accesible', () => {
    const fixture = TestBed.createComponent(Host);
    fixture.componentInstance.open.set(true);
    fixture.detectChanges();
    const dialog = document.querySelector('[role="dialog"]');

    expect(dialog).not.toBeNull();
    expect(dialog?.getAttribute('aria-labelledby')).toBeTruthy();
    expect(dialog?.textContent).toContain('Eliminar camión');
  });

  it('emite la confirmación al pulsar el botón de confirmar', () => {
    const fixture = TestBed.createComponent(Host);
    fixture.componentInstance.open.set(true);
    fixture.detectChanges();
    const confirm = Array.from(
      document.querySelectorAll<HTMLButtonElement>('[role="dialog"] button'),
    ).find((button) => button.textContent?.includes('Eliminar') ?? false);

    confirm?.click();
    fixture.detectChanges();

    expect(fixture.componentInstance.confirmed()).toBe(1);
  });
});
