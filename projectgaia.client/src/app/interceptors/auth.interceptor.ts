import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

/**
 * Interceta todos os pedidos HTTP e injeta o bearer token de autenticação do cliente em todos esses pedidos
 * Garante que apenas pedidos com token sejam válidos e evita a repetição dos headers em todos os pedidos GET ou POST
 * @param req O request HTTP por parte do utilizador
 * @param next A função que continua o fluxo de requests
 * @returns O pedido a ser processado
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const authToken = authService.getToken();

  if (authToken) {
    const clonedRequest = req.clone({
      setHeaders: {
        Authorization: `Bearer ${authToken}`
      }
    });
    return next(clonedRequest);
  }

  return next(req);
};
