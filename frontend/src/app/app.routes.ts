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
    loadComponent: () => import('./features/matches/matches').then((m) => m.Matches),
    title: 'Partidos · Polla Mundialista',
  },
  {
    path: 'standings',
    canActivate: [authGuard],
    loadComponent: () => import('./features/standings/standings').then((m) => m.Standings),
    title: 'Grupos · Polla Mundialista',
  },
  {
    path: 'leaderboard',
    canActivate: [authGuard],
    loadComponent: () => import('./features/leaderboard/leaderboard').then((m) => m.Leaderboard),
    title: 'Tabla · Polla Mundialista',
  },
  {
    path: 'users/:id',
    canActivate: [authGuard],
    loadComponent: () => import('./features/users/user-history').then((m) => m.UserHistoryPage),
    title: 'Historial · Polla Mundialista',
  },
  {
    path: 'admin',
    canActivate: [adminGuard],
    loadComponent: () => import('./features/admin/admin').then((m) => m.Admin),
    title: 'Admin · Polla Mundialista',
  },
  { path: '**', redirectTo: '' },
];
