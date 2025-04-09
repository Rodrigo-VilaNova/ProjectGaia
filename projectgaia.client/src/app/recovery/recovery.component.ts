import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { environment } from '../../environments/environment';
import { CommonModule } from '@angular/common';
import { FooterComponent } from '../footer/footer.component';
import { NavbarSimpleComponent } from '../navbar-simple/navbar-simple.component';

@Component({
  selector: 'app-recovery',
  templateUrl: './recovery.component.html',
  styleUrls: ['./recovery.component.css'],
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule,NavbarSimpleComponent, FooterComponent],
})

/**
 * Componente responsável pela recuperação de password
 */
export class RecoveryComponent {

  /** Formulário de recuperaçáo de password */
  emailForm: FormGroup;

  /** Averigua se o formulário está a ser submetido */
  loading = false;

  /** Resposta do servidor */
  serverResponse: string | null = null;

  /** Resposta do servidor em caso de erro */
  serverError: string | null = null;

  /**
   * Construtor do componente
   * @param fb FormBuilder para criação e validação do formulário
   * @param http Cliente HTTP para comunicação com a API
   * @param router Serviço de routing para navegação
   */
  constructor(private fb: FormBuilder, private http: HttpClient, private router: Router) {
    // Criação do formulário com validações
    this.emailForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]]
    });
  }

  /**
   * Função de submit após inserção da nova password
   */
  onSubmit() {
    if (this.emailForm.invalid) {
      return;
    }

    this.serverResponse = null;
    this.serverError = null;
    this.loading = true;
    
    const recoveryDTO = { email: this.emailForm.value.email };

    this.http.post(`${environment.apiUrl}/account/recovery`, recoveryDTO, { responseType: 'text' })
      .subscribe({
        next: (response: string) => {
          this.serverResponse = response;
          this.loading = false;
        },
        error: (error) => {
          this.loading = false;
          this.serverError = error.error || `An unexpected error occured with no message, error code ${error.status}`;
        }
      });
  }

  /**
   * Navega para a página principal
   */
  goToLandingPage() {
    this.router.navigate(['/']);
  }
}
