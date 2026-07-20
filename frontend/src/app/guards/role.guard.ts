import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

/**
 * Role guard factory — pass allowed roles, returns a CanActivateFn.
 * Usage: canActivate: [authGuard, roleGuard('Admin')]
 *        canActivate: [authGuard, roleGuard('Admin', 'Staff')]
 */
export const roleGuard = (...allowedRoles: string[]): CanActivateFn => {
  return (_route, _state) => {
    const auth   = inject(AuthService);
    const router = inject(Router);

    const user = auth.currentUser();
    if (!user) {
      router.navigate(['/login']);
      return false;
    }

    if (allowedRoles.includes(user.role)) {
      return true;
    }

    // Redirect patients to their portal, others to admin dashboard
    if (user.role === 'Patient') {
      router.navigate(['/user-portal']);
    } else {
      router.navigate(['/']);
    }
    return false;
  };
};
