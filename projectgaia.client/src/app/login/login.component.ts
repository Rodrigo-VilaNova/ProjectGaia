import { Component } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { AuthService } from '../services/auth.service';
import { environment } from '../../environments/environment';
import { FooterComponent } from '../footer/footer.component';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
  standalone: true,
  imports: [RouterModule, ReactiveFormsModule, CommonModule, FooterComponent]
})

/**
 * Componente responsável pelo login
 */
export class LoginComponent {

  /** Formulário de login */
  loginForm: FormGroup;

  /** Averigua se o login está a ser processado */
  loading = false;

  /** Mensagem de erro exibida ao utilizador */
  errorMessage: string | null = null;

  /**
   * Construtor do componente
   * @param fb FormBuilder para criação e validação do formulário
   * @param http Cliente HTTP para comunicação com a API
   * @param router Serviço de routing para navegação
   * @param authService Serviço de autenticação
   */
  constructor(private fb: FormBuilder, private http: HttpClient, private router: Router, private authService: AuthService) {
    // Criação do formulário com validações
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required]]
    });
  }

  /**
   * Função de submit após inserção das credenciais de login
   */
  onSubmit() {
    if (this.loginForm.invalid) {
      return;
    }

    this.loading = true;
    this.errorMessage = null;

    const credentials = {
      email: this.loginForm.value.email,
      password: this.loginForm.value.password
    };

    this.http.post<LoginResponse>(`${environment.apiUrl}/account/login`, credentials)
      .subscribe({
        next: response => {
          this.authService.setToken(response.Token);

          this.router.navigate(['/dashboard']);
        },
        error: (error: HttpErrorResponse) => {
          this.loading = false;

          this.errorMessage = error.error || `An unexpected error occured with no message, error code ${error.status}`;
          console.log('Error Status Code:', error.status, 'Response:', error.error);
        }
      });
  }

  /**
   * Navega para a landing page
   */
  goToLandingPage() {
    this.router.navigate(['']);
  }

  /**
   * Navega para a página de recuperação de password
   */
  navigateToForgotPassword() {
    this.router.navigate(['recovery']);
  }
}

/**
 * Interface de resposta de login que consiste no token de autenticação do login
 */
interface LoginResponse {
  Token: string;
}
