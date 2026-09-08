import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

type AppRole = 'customer' | 'organizer' | 'admin';

function currentRole(): AppRole | null {
  try {
    return sessionStorage.getItem('eventora-role') as AppRole | null;
  } catch {
    return null;
  }
}

function roleGuard(required?: AppRole): CanActivateFn {
  return () => {
    const router = inject(Router);
    const role = currentRole();
    if (!role) return router.createUrlTree(['/login']);
    if (required && role !== required) {
      const target =
        role === 'admin'
          ? '/admin/dashboard'
          : role === 'organizer'
            ? '/organizer/dashboard'
            : '/customer/dashboard';
      return router.createUrlTree([target]);
    }
    return true;
  };
}

export const authGuard = roleGuard();
export const customerGuard = roleGuard('customer');
export const organizerGuard = roleGuard('organizer');
export const adminGuard = roleGuard('admin');
