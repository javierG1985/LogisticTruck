import { Routes } from '@angular/router';

// Solo existe fuera de producción: app.routes.ts no la registra en un build de producción.
export const STYLE_GUIDE_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/style-guide-page/style-guide-page').then((page) => page.StyleGuidePage),
  },
];
