import { ChangeDetectionStrategy, Component } from '@angular/core';
import { EmptyState } from '@shared/ui/empty-state/empty-state';
import { KpiCard } from '@shared/ui/kpi-card/kpi-card';
import { PageHeader } from '@shared/ui/page-header/page-header';

interface PendingKpi {
  readonly label: string;
  readonly icon: string;
}

// Métricas que la Torre de Control mostrará cuando existan datos. Sin valores: nunca se inventan cifras.
const KPIS: readonly PendingKpi[] = [
  { label: 'Camiones disponibles', icon: 'local_shipping' },
  { label: 'Conductores activos', icon: 'badge' },
  { label: 'Asignaciones activas', icon: 'assignment_ind' },
  { label: 'Vencimientos próximos', icon: 'event_busy' },
];

@Component({
  selector: 'lt-dashboard-page',
  imports: [EmptyState, KpiCard, PageHeader],
  templateUrl: './dashboard-page.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardPage {
  protected readonly kpis = KPIS;
}
