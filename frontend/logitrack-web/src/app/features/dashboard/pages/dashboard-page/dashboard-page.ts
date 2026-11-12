import { ChangeDetectionStrategy, Component } from '@angular/core';
import { PageHeader } from '@shared/ui/page-header/page-header';

// Se completa en el paso 6 con la Torre de Control vacía.
@Component({
  selector: 'lt-dashboard-page',
  imports: [PageHeader],
  templateUrl: './dashboard-page.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardPage {}
