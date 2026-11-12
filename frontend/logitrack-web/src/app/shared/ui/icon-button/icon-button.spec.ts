import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { IconButton } from './icon-button';

@Component({
  imports: [IconButton],
  template: `<lt-icon-button icon="notifications" label="Ver notificaciones" />`,
})
class Host {}

describe('IconButton', () => {
  it('expone un nombre accesible aunque no tenga texto visible', () => {
    const fixture = TestBed.createComponent(Host);
    fixture.detectChanges();
    const button: HTMLButtonElement = fixture.nativeElement.querySelector('button');

    expect(button.getAttribute('aria-label')).toBe('Ver notificaciones');
    expect(button.textContent?.trim()).toBe('notifications');
  });
});
