import { Routes } from '@angular/router';
import { environment } from '@env/environment';
import { NotFoundPage } from '@core/layout/not-found-page/not-found-page';
import { Shell } from '@core/layout/shell/shell';

// La guía de estilo se registra solo fuera de producción.
const DEVELOPMENT_ROUTES: Routes = environment.production
  ? []
  : [
      {
        path: 'style-guide',
        title: 'Guía de estilo · LogiTrack',
        loadChildren: () =>
          import('@features/style-guide/style-guide.routes').then(
            (routes) => routes.STYLE_GUIDE_ROUTES,
          ),
      },
    ];

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
  {
    path: '',
    component: Shell,
    children: [
      {
        path: 'dashboard',
        title: 'Torre de Control · LogiTrack',
        loadChildren: () =>
          import('@features/dashboard/dashboard.routes').then((routes) => routes.DASHBOARD_ROUTES),
      },
      ...DEVELOPMENT_ROUTES,
      { path: '**', title: 'Página no encontrada · LogiTrack', component: NotFoundPage },
    ],
  },
];
