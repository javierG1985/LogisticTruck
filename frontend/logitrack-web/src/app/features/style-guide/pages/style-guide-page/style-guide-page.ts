import { ChangeDetectionStrategy, Component } from '@angular/core';
import { PageHeader } from '@shared/ui/page-header/page-header';

// Se completa en el paso 6 con tokens, componentes y la composición de ejemplo.
@Component({
  selector: 'lt-style-guide-page',
  imports: [PageHeader],
  templateUrl: './style-guide-page.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StyleGuidePage {}
