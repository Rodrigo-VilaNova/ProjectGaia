import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { AuthService } from './interceptors/auth.service';

@Injectable({
  providedIn: 'root'
})

/**
 * Componente responsável pela segurança da aplicação
 * Apenas utilizadores com tokens de autenticação válidos conseguem aceder a outras páginas para além da landing page
 */
export class AuthGuard implements CanActivate {

  constructor(private authService: AuthService, private router: Router) { }

  canActivate(
    next: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): Observable<boolean> | Promise<boolean> | boolean {

    if (!this.authService.hasToken()) {
      this.router.navigate(['/landing']);
      return false;
    }

    return true;
  }
}
