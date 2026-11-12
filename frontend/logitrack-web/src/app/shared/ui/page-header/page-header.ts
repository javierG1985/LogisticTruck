import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { Badge } from '../badge/badge';
import { Icon } from '../icon/icon';

// Título de página con migas de pan. Las acciones se apilan en celular y van a la derecha en pantallas anchas.
@Component({
  selector: 'lt-page-header',
  imports: [Badge, Icon],
  templateUrl: './page-header.html',
  host: { class: 'block @container' },
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PageHeader {
  readonly breadcrumbs = input<string[]>([]);
  readonly title = input.required<string>();
  readonly badge = input<string | null>(null);
}
