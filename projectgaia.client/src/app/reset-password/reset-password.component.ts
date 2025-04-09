import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors, ReactiveFormsModule } from '@angular/forms';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { ActivatedRoute, Router } from '@angular/router';
import { environment } from '../../environments/environment';
import { CommonModule } from '@angular/common';
import { FooterComponent } from '../footer/footer.component';
import { NavbarSimpleComponent } from '../navbar-simple/navbar-simple.component';

@Component({
  selector: 'app-reset-password',
  templateUrl: './reset-password.component.html',
  styleUrls: ['./reset-password.component.css'],
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule, NavbarSimpleComponent, FooterComponent]
})

/**
 * Componente responsável pelo reset da password
 */
export class ResetPasswordComponent {

  /** Formulário para dar reset à password */
  resetPasswordForm: FormGroup;

  /** Averigua se o processo está a ser processado */
  loading = false;

  /** Mensagem de erro exibida ao utilizador */
  errorMessage: string | null = null;

  /** Mensagem de sucesso exibida ao utilizador */
  successMessage: string | null = null;

  /**
   * Contrutor do componente
   * @param fb FormBuilder para criação e validação do formulário
   * @param http Cliente HTTP para comunicação com a API
   * @param router Serviço de routing para navegação
   * @param activatedRoute A rota onde o token de autenticação vai ser buscado
   */
  constructor(private fb: FormBuilder, private http: HttpClient, private router: Router, private activatedRoute: ActivatedRoute) {
    // Criação do formulário com validações
    this.resetPasswordForm = this.fb.group({
      password: ['', [Validators.required, Validators.minLength(8), Validators.maxLength(128), this.passwordStrengthValidator]],
      confirmPassword: ['', Validators.required]
    }, {
      validators: this.passwordMatchValidator
    });
  }

  /**
   * Averigua se a password e a confirmação da password são iguais
   * @param group O grupo dos inputs respetivos
   * @returns Um erro de validação caso exista, null caso sejam iguais
   */
  passwordMatchValidator(group: AbstractControl): ValidationErrors | null {
    const password = group.get('password')?.value;
    const confirmPassword = group.get('confirmPassword')?.value;
    return password === confirmPassword ? null : { passwordMismatch: true };
  }

  /**
   * Averigua se a password é segura
   * @param control O input da password
   * @returns Um erro de validação caso exista, ou null se for segura
   */
  passwordStrengthValidator(control: AbstractControl): ValidationErrors | null {
    const value = control.value;
    if (!value) return null;
    const hasUpperCase = /[A-Z]/.test(value);
    const hasLowerCase = /[a-z]/.test(value);
    const hasNumber = /\d/.test(value);
    const hasSpecialChar = /[!@#$%^&*(),.?":{}|<>]/.test(value);
    const valid = hasUpperCase && hasLowerCase && hasNumber && hasSpecialChar;
    return valid ? null : { passwordStrength: true };
  }

  /**
   * Função de submit após inserção das passwords
   */
  onSubmit() {
    if (this.resetPasswordForm.invalid) {
      this.errorMessage = 'Please correct the errors in the form.';
      return;
    }

    this.loading = true;
    this.errorMessage = null;
    this.successMessage = null;

    const resetDTO = {
      token: this.activatedRoute.snapshot.queryParamMap.get('token') || "0",
      password: this.resetPasswordForm.value.password
    };

    this.http.put(`${environment.apiUrl}/account/reset`, resetDTO, { responseType: 'text' })
      .subscribe({
        next: (response: string) => {
          alert(response);
          this.router.navigate(['']);
        },
        error: (error: HttpErrorResponse) => {
          this.loading = false;

          this.errorMessage = error.error || `An unexpected error occured with no message, error code ${error.status}`;
          console.log('Error Status Code:', error.status, 'Response:', error.error);
        }
      });
  }
}
