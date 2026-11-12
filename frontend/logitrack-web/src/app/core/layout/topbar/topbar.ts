import { ChangeDetectionStrategy, Component, inject, output } from '@angular/core';
import { SegmentedControl, SegmentedOption } from '@shared/ui/segmented-control/segmented-control';
import { SelectField } from '@shared/ui/select-field/select-field';
import { SearchInput } from '@shared/ui/search-input/search-input';
import { IconButton } from '@shared/ui/icon-button/icon-button';
import { ThemeMode, ThemeService } from '../../theme/theme-service';
import { UserMenu } from '../user-menu/user-menu';

const THEME_OPTIONS: readonly SegmentedOption[] = [
  { value: 'light', label: 'Claro' },
  { value: 'dark', label: 'Oscuro' },
  { value: 'system', label: 'Sistema' },
];

// Barra superior translúcida. El buscador global y las notificaciones aún no existen: van deshabilitados.
@Component({
  selector: 'lt-topbar',
  imports: [IconButton, SearchInput, SegmentedControl, SelectField, UserMenu],
  templateUrl: './topbar.html',
  host: { class: 'block' },
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Topbar {
  readonly menuRequested = output<void>();

  protected readonly theme = inject(ThemeService);
  protected readonly themeOptions = THEME_OPTIONS;

  protected setTheme(value: string): void {
    this.theme.set(value as ThemeMode);
  }
}
