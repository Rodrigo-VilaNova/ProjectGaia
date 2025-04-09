import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})

/**
 * Serviço de autenticação
 * Responsável por gerir os tokens de autenticação dos utilizadores
 */
export class AuthService {
  private tokenKey = 'auth_token';

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  setToken(token: string): void {
    localStorage.setItem(this.tokenKey, token);
  }

  removeToken(): void {
    localStorage.removeItem(this.tokenKey);
  }

  hasToken(): boolean {
    return !!localStorage.getItem(this.tokenKey);
  }
}
