import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

/** Allows access only to authenticated users; otherwise redirects to /login. */
export const authGuard: CanActivateFn = (_route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.isAuthenticated()) return true;
  return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
};

/** Allows access only to Admins; non-admins are sent to the dashboard. */
export const adminGuard: CanActivateFn = (_route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.isAuthenticated())
    return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
  if (auth.isAdmin()) return true;
  return router.createUrlTree(['/']);
};
