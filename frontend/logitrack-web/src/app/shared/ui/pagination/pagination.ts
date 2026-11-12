import { ChangeDetectionStrategy, Component, computed, input, model } from '@angular/core';
import { Button } from '../button/button';

const PAGE_BASE = 'min-h-11 min-w-11 rounded-lg font-label-md text-label-md transition-colors';
const PAGE_CURRENT = 'bg-primary text-on-primary';
const PAGE_OTHER = 'text-on-surface-variant hover:bg-surface-container-high hover:text-on-surface';

// Anterior y siguiente, páginas cercanas y el total. En celular solo muestra "Página X de Y".
@Component({
  selector: 'lt-pagination',
  imports: [Button],
  templateUrl: './pagination.html',
  host: { class: 'block' },
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Pagination {
  readonly totalPages = input.required<number>();
  readonly totalCount = input<number | null>(null);
  readonly page = model(1);

  protected readonly visiblePages = computed(() => {
    const total = this.totalPages();
    const current = this.page();
    const start = Math.max(1, Math.min(current - 2, total - 4));
    const end = Math.min(total, start + 4);
    return Array.from({ length: end - start + 1 }, (_, index) => start + index);
  });

  protected pageClasses(page: number): string {
    return `${PAGE_BASE} ${page === this.page() ? PAGE_CURRENT : PAGE_OTHER}`;
  }

  protected go(page: number): void {
    if (page >= 1 && page <= this.totalPages()) {
      this.page.set(page);
    }
  }
}
