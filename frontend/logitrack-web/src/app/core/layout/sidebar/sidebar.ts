import { CdkTrapFocus } from '@angular/cdk/a11y';
import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { Badge } from '@shared/ui/badge/badge';
import { Icon } from '@shared/ui/icon/icon';
import { IconButton } from '@shared/ui/icon-button/icon-button';
import { APP_VERSION } from '../../config/app-version';
import {
  NAVIGATION_ITEMS,
  NAVIGATION_SECTIONS,
  NavItem,
  NavSection,
} from '../../navigation/navigation-items';

interface NavGroup {
  readonly section: NavSection;
  readonly items: readonly NavItem[];
}

const GROUPS: readonly NavGroup[] = NAVIGATION_SECTIONS.map((section) => ({
  section,
  items: NAVIGATION_ITEMS.filter((item) => item.section === section),
}));

const LINK_BASE =
  'flex min-h-11 items-center gap-space-sm rounded-lg px-space-md font-label-md text-label-md transition-colors';

// Móvil: panel deslizante con foco atrapado. Escritorio: barra fija de 256 px o de 72 px si está colapsada.
@Component({
  selector: 'lt-sidebar',
  imports: [CdkTrapFocus, RouterLink, RouterLinkActive, Badge, Icon, IconButton],
  templateUrl: './sidebar.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Sidebar {
  readonly collapsed = input(false);
  readonly mobileOpen = input(false);

  readonly navigated = output<void>();
  readonly closeRequested = output<void>();
  readonly toggleRequested = output<void>();

  protected readonly groups = GROUPS;
  protected readonly version = APP_VERSION;
  protected readonly linkBase = LINK_BASE;

  protected readonly panelClasses = computed(() => {
    const position = this.mobileOpen() ? 'translate-x-0' : '-translate-x-full';
    const width = this.collapsed() ? 'lg:w-18' : 'lg:w-64';

    return `fixed inset-y-0 left-0 z-50 flex w-64 flex-col justify-between overflow-y-auto bg-surface-container-low py-space-md shadow-soft transition-transform duration-200 motion-reduce:transition-none lg:z-30 lg:translate-x-0 ${position} ${width}`;
  });
}
