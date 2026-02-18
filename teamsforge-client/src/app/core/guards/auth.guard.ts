import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { Authservice } from '../services/authservice';

export const authGuard = () => {
  const authService = inject(Authservice);
  const router = inject(Router);

  if (authService.isLoggedIn()) {
    return true;
  }

  router.navigate(['/login']);
  return false;
};
