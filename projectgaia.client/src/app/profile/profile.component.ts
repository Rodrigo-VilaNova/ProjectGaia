import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../interceptors/auth.service';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { NavbarComponent } from '../navbar/navbar.component';


@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.css'],
  standalone: true,
  imports: [NavbarComponent]
})

/**
 * Componente responsável pela conta do utilizador
 */
export class ProfileComponent {

  /**
   * Construtor do componente
   * @param router Serviço de routing para navegação
   * @param http Cliente HTTP para comunicação com a API
   * @param authService Serviço de autenticação
   */
  constructor(private router: Router, private http: HttpClient, private authService: AuthService) { }

  /**
   * Navega para a página de dashboard
   */
  goBack() {
    this.router.navigate(['/dashboard']);
  }

  /**
   * Limpa o token de autenticação do utilizador
   */
  clearToken() {
    this.authService.removeToken();
    this.router.navigate(['']);
  }

  /**
   * Navega para a página de alterar password
   */
  changePassword() {
    this.router.navigate(['/change-password']);
  }

  /**
   * Dá logout da conta do utilizador
   */
  logoutAccount() {
    if (confirm("Are you sure you wish to logout?")) {
      this.http.delete(`${environment.apiUrl}/account/logout`, { responseType: 'text' })
        .subscribe(
          () => {
            this.clearToken();
          },
          (error) => {
            console.error('Error logging out:', error);
            alert('Failed to logout. Please try again.');
          }
        );
    }
  }

  /**
   * Apaga a conta do utilizador da base de dados
   */
  deleteAccount() {
    if (confirm('Are you sure you want to delete your account? This action cannot be undone.')) {
      this.http.delete(`${environment.apiUrl}/account/delete`, { responseType: 'text' })
        .subscribe(
          () => {
            alert('Account deleted successfully.');
            this.clearToken();
          },
          (error) => {
            console.error('Error deleting account:', error);
            alert('Failed to delete account. Please try again.');
          }
        );
    }
  }
}
