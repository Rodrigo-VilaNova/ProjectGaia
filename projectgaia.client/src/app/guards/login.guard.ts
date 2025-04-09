import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})

/**
 * Componente responsável pela segurança da aplicação
 * Redireciona utilizadores com token de autenticação para a dashboard
 */
export class LoginGuard implements CanActivate {

  constructor(private authService: AuthService, private router: Router) { }

  canActivate(
    next: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): Observable<boolean> | Promise<boolean> | boolean {

    if (this.authService.hasToken()) {
      this.router.navigate(['/dashboard']);
      return false;
    }

    return true;
  }
}
