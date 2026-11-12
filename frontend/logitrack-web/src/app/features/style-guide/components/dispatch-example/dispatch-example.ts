import { ChangeDetectionStrategy, Component, signal } from '@angular/core';
import { Badge, BadgeVariant } from '@shared/ui/badge/badge';
import { Button } from '@shared/ui/button/button';
import { Card } from '@shared/ui/card/card';
import { EmptyState } from '@shared/ui/empty-state/empty-state';
import { FilterBar } from '@shared/ui/filter-bar/filter-bar';
import { KpiCard, KpiTone } from '@shared/ui/kpi-card/kpi-card';
import { PageHeader } from '@shared/ui/page-header/page-header';
import { ProgressBar } from '@shared/ui/progress-bar/progress-bar';
import { SearchInput } from '@shared/ui/search-input/search-input';
import { SectionHeader } from '@shared/ui/section-header/section-header';
import { SegmentedControl, SegmentedOption } from '@shared/ui/segmented-control/segmented-control';
import { SelectField, SelectOption } from '@shared/ui/select-field/select-field';

interface ExampleOrder {
  readonly id: string;
  readonly client: string;
  readonly address: string;
  readonly priority: string;
  readonly priorityTone: BadgeVariant;
  readonly window: string;
  readonly weight: string;
}

interface ExampleVehicle {
  readonly plate: string;
  readonly type: string;
  readonly driver: string;
  readonly license: string;
  readonly city: string;
  readonly capacity: number;
  readonly load: string;
}

interface ExampleKpi {
  readonly label: string;
  readonly value: string;
  readonly support: string;
  readonly tone: KpiTone;
  readonly icon: string;
}

// Datos ficticios para comparar con el diseño de Stitch. Nada de esto viene de una API.
const ORDERS: readonly ExampleOrder[] = [
  {
    id: 'ORD-8942-CO',
    client: 'Tienda Central Los Andes',
    address: 'Calle 80 # 68-20, Bogotá',
    priority: 'Alta',
    priorityTone: 'danger',
    window: '08:30 - 11:00',
    weight: '4,2 t',
  },
  {
    id: 'ORD-8945-CO',
    client: 'Droguería La Esperanza Norte',
    address: 'Autopista Norte # 170-35, Bogotá',
    priority: 'Normal',
    priorityTone: 'neutral',
    window: '10:00 - 13:00',
    weight: '1,8 t',
  },
  {
    id: 'ORD-8949-CO',
    client: 'Laboratorios Andinos S.A.S.',
    address: 'Av. Boyacá # 13-70, Bogotá',
    priority: 'Alta',
    priorityTone: 'danger',
    window: '11:30 - 14:00',
    weight: '2,1 t',
  },
];

const VEHICLE: ExampleVehicle = {
  plate: 'TKR482',
  type: 'Rígido, furgón cerrado',
  driver: 'Carlos Pérez',
  license: 'C3',
  city: 'Bogotá',
  capacity: 85,
  load: '2,4 / 2,8 t',
};

const KPIS: readonly ExampleKpi[] = [
  {
    label: 'Cubicaje optimizado',
    value: '94,2%',
    support: '+3,8% vs. semana',
    tone: 'positive',
    icon: 'view_in_ar',
  },
  {
    label: 'Ventanas cumplidas',
    value: '98,1%',
    support: 'Meta: 95%',
    tone: 'positive',
    icon: 'verified',
  },
  {
    label: 'Disponibilidad de flota',
    value: '10 / 12',
    support: '2 en mantenimiento',
    tone: 'neutral',
    icon: 'garage_home',
  },
  {
    label: 'Sobrecargas',
    value: '0',
    support: 'Control de peso por eje',
    tone: 'neutral',
    icon: 'scale',
  },
];

const HUBS: readonly SelectOption[] = [
  { value: 'bogota', label: 'Hub Central Bogotá (Fontibón)' },
  { value: 'medellin', label: 'Hub Medellín (Itagüí)' },
];

const VIEWS: readonly SegmentedOption[] = [
  { value: 'cards', label: 'Vista Tarjetas' },
  { value: 'matrix', label: 'Matriz Horaria' },
];

// Reproduce la pantalla "Despachos y Asignación" de Stitch usando solo componentes del kit. Todo está marcado como ejemplo.
@Component({
  selector: 'lt-dispatch-example',
  imports: [
    Badge,
    Button,
    Card,
    EmptyState,
    FilterBar,
    KpiCard,
    PageHeader,
    ProgressBar,
    SearchInput,
    SectionHeader,
    SegmentedControl,
    SelectField,
  ],
  templateUrl: './dispatch-example.html',
  host: { class: 'block' },
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DispatchExample {
  protected readonly orders = ORDERS;
  protected readonly vehicle = VEHICLE;
  protected readonly kpis = KPIS;
  protected readonly hubs = HUBS;
  protected readonly views = VIEWS;

  protected readonly searchTerm = signal('');
  protected readonly hub = signal('bogota');
  protected readonly view = signal('cards');
}
