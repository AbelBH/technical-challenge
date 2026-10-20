import { Routes } from '@angular/router';
import { authGuard, guestGuard } from './core/auth/auth.guards';

// Las pantallas se cargan de forma diferida (lazy loading): solo se descargan cuando se visitan.
export const routes: Routes = [
  {
    path: 'login',
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./features/auth/login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./layout/shell/shell.component').then((m) => m.ShellComponent),
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'converter' },
      {
        path: 'converter',
        title: 'Conversión | Tipo de Cambio',
        loadComponent: () =>
          import('./features/converter/converter.component').then((m) => m.ConverterComponent),
      },
      {
        path: 'exchange-rates',
        title: 'Tipos de cambio | Tipo de Cambio',
        loadComponent: () =>
          import('./features/exchange-rates/exchange-rates.component').then(
            (m) => m.ExchangeRatesComponent,
          ),
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
