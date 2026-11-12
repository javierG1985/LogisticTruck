import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Pagination } from './pagination';

@Component({
  imports: [Pagination],
  template: `<lt-pagination [totalPages]="10" [totalCount]="195" [(page)]="page" />`,
})
class Host {
  readonly page = signal(1);
}

describe('Pagination', () => {
  it('no permite ir antes de la primera página', () => {
    const fixture = TestBed.createComponent(Host);
    fixture.detectChanges();
    const previous: HTMLButtonElement = Array.from<HTMLButtonElement>(
      fixture.nativeElement.querySelectorAll('button'),
    ).find((button) => button.textContent?.includes('Anterior') ?? false)!;

    expect(previous.disabled).toBe(true);
  });

  it('marca la página actual y avanza con siguiente', () => {
    const fixture = TestBed.createComponent(Host);
    fixture.detectChanges();
    const next: HTMLButtonElement = Array.from<HTMLButtonElement>(
      fixture.nativeElement.querySelectorAll('button'),
    ).find((button) => button.textContent?.includes('Siguiente') ?? false)!;

    next.click();
    fixture.detectChanges();

    expect(fixture.componentInstance.page()).toBe(2);
    const current = fixture.nativeElement.querySelector('[aria-current="page"]');
    expect(current?.textContent?.trim()).toBe('2');
  });
});
