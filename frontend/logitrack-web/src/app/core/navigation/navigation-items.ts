// Menú principal declarado en un solo lugar. Sidebar y menú móvil lo leen de aquí.
// Agregar una feature al menú es agregar una línea. Lo que aún no existe va con enabled: false ("Próximamente").

export type NavSection = 'Operación' | 'Flota' | 'Administración';

export interface NavItem {
  readonly label: string;
  readonly icon: string;
  readonly section: NavSection;
  readonly enabled: boolean;
  readonly path?: string;
}

export const NAVIGATION_SECTIONS: readonly NavSection[] = ['Operación', 'Flota', 'Administración'];

export const NAVIGATION_ITEMS: readonly NavItem[] = [
  {
    label: 'Torre de Control',
    icon: 'monitoring',
    section: 'Operación',
    enabled: true,
    path: '/dashboard',
  },
  { label: 'Despachos y Asignación', icon: 'route', section: 'Operación', enabled: false },
  { label: 'Seguimiento', icon: 'map', section: 'Operación', enabled: false },
  { label: 'Camiones', icon: 'local_shipping', section: 'Flota', enabled: false },
  { label: 'Conductores', icon: 'badge', section: 'Flota', enabled: false },
  { label: 'Asignaciones', icon: 'assignment_ind', section: 'Flota', enabled: false },
  { label: 'Documentos y firmas', icon: 'fact_check', section: 'Administración', enabled: false },
  {
    label: 'Facturación y tarifas',
    icon: 'request_quote',
    section: 'Administración',
    enabled: false,
  },
];
