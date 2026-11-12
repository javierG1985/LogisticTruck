import { ChangeDetectionStrategy, Component, signal } from '@angular/core';
import { Badge, BadgeVariant } from '@shared/ui/badge/badge';
import { Button, ButtonVariant } from '@shared/ui/button/button';
import { Card } from '@shared/ui/card/card';
import { ConfirmDialog } from '@shared/ui/confirm-dialog/confirm-dialog';
import { EmptyState } from '@shared/ui/empty-state/empty-state';
import { FilterBar } from '@shared/ui/filter-bar/filter-bar';
import { IconButton } from '@shared/ui/icon-button/icon-button';
import { KpiCard, KpiTone } from '@shared/ui/kpi-card/kpi-card';
import { Pagination } from '@shared/ui/pagination/pagination';
import { PageHeader } from '@shared/ui/page-header/page-header';
import { ProgressBar } from '@shared/ui/progress-bar/progress-bar';
import { SearchInput } from '@shared/ui/search-input/search-input';
import { SectionHeader, SectionTone } from '@shared/ui/section-header/section-header';
import { SegmentedControl, SegmentedOption } from '@shared/ui/segmented-control/segmented-control';
import { SelectField, SelectOption } from '@shared/ui/select-field/select-field';
import { Skeleton } from '@shared/ui/skeleton/skeleton';

const BUTTON_VARIANTS: readonly ButtonVariant[] = [
  'primary',
  'accent',
  'secondary',
  'ghost',
  'danger',
];
const BADGE_VARIANTS: readonly BadgeVariant[] = ['neutral', 'info', 'success', 'warning', 'danger'];
const KPI_TONES: readonly KpiTone[] = ['positive', 'neutral', 'negative'];
const SECTION_TONES: readonly SectionTone[] = ['primary', 'accent'];

const HUBS: readonly SelectOption[] = [
  { value: 'bogota', label: 'Hub Central Bogotá (Fontibón)' },
  { value: 'medellin', label: 'Hub Medellín (Itagüí)' },
  { value: 'cali', label: 'Hub Cali (Yumbo)' },
];

const VIEWS: readonly SegmentedOption[] = [
  { value: 'cards', label: 'Vista Tarjetas' },
  { value: 'matrix', label: 'Matriz Horaria' },
];

// Galería de todo el kit en un solo lugar. Los controles con estado usan signals para mostrar su comportamiento real.
@Component({
  selector: 'lt-component-gallery',
  imports: [
    Badge,
    Button,
    Card,
    ConfirmDialog,
    EmptyState,
    FilterBar,
    IconButton,
    KpiCard,
    Pagination,
    PageHeader,
    ProgressBar,
    SearchInput,
    SectionHeader,
    SegmentedControl,
    SelectField,
    Skeleton,
  ],
  templateUrl: './component-gallery.html',
  host: { class: 'block' },
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ComponentGallery {
  protected readonly buttonVariants = BUTTON_VARIANTS;
  protected readonly badgeVariants = BADGE_VARIANTS;
  protected readonly kpiTones = KPI_TONES;
  protected readonly sectionTones = SECTION_TONES;
  protected readonly hubs = HUBS;
  protected readonly views = VIEWS;

  protected readonly searchTerm = signal('');
  protected readonly hub = signal('bogota');
  protected readonly view = signal('cards');
  protected readonly page = signal(3);
  protected readonly confirmOpen = signal(false);
  protected readonly confirmResult = signal('Todavía no se ha confirmado nada.');
}
