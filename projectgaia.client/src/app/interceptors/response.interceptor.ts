import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';
import { catchError } from 'rxjs';
import { throwError } from 'rxjs';

/**
 * Intercepta a resposta de todos os pedidos HTTP e, em caso de erro 401 ou 403,
 * remove o token de autenticação e redireciona o utilizador para a página inicial.
 * @param req O request HTTP por parte do utilizador
 * @param next A função que continua o fluxo de requests
 * @returns O pedido a ser processado
 */
export const responseInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 || error.status === 403) {
        authService.removeToken();
        router.navigate(['']);
      }

      return throwError(() => error);
    })
  );
};

