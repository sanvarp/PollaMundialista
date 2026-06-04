import { Routes } from '@angular/router';
import { adminGuard, authGuard } from './core/auth/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login').then((m) => m.Login),
    title: 'Entrar · Polla Mundialista',
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register/register').then((m) => m.Register),
    title: 'Crear cuenta · Polla Mundialista',
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./features/dashboard/dashboard').then((m) => m.Dashboard),
    title: 'Inicio · Polla Mundialista',
  },
  {
    path: 'admin',
    canActivate: [adminGuard],
    loadComponent: () => import('./features/admin/admin').then((m) => m.Admin),
    title: 'Admin · Polla Mundialista',
  },
  { path: '**', redirectTo: '' },
];
