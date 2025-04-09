import { Component } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { environment } from '../../environments/environment';
import { FooterComponent } from '../footer/footer.component';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css'],
  standalone: true,
  imports: [RouterModule, ReactiveFormsModule, CommonModule, FooterComponent]
})

/**
 * Componente responsável pelo registo de um utilizador
 */
export class RegisterComponent {

  /** Formulário de registo */
  registerForm: FormGroup;

  /** Averigua se o registo está a ser processado */
  loading = false;

  /** Mensagem de erro exibida ao utilizador */
  errorMessage: string | null = null;

  /**
   * Contrutor do componente
   * @param fb FormBuilder para criação e validação do formulário
   * @param http Cliente HTTP para comunicação com a API
   * @param router Serviço de routing para navegação
   */
  constructor(private fb: FormBuilder, private http: HttpClient, private router: Router) {
    // Criação do formulário com validações
    this.registerForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(64)]],
      email: ['', [Validators.required, Validators.email]],
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
   * Função de submit após inserção das credenciais de registo
   */
  onSubmit() {
    if (this.registerForm.invalid) {
      this.errorMessage = 'Please correct the errors in the form.';
      return;
    }
    this.loading = true;
    this.errorMessage = null;

    const accountDTO = {
      name: this.registerForm.value.name,
      email: this.registerForm.value.email,
      password: this.registerForm.value.password
    };

    this.http.post(`${environment.apiUrl}/account/register`, accountDTO, { observe: 'response', responseType: 'text' })
      .subscribe({
        next: response => {
          console.log('Status Code:', response.status);
          this.router.navigate(['/register-sent']);
        },
        error: (error: HttpErrorResponse) => {
          this.loading = false;

          this.errorMessage = error.error || `An unexpected error occured with no message, error code ${error.status}`;
          console.log('Error Status Code:', error.status, 'Response:', error.error);
        }
      });
  }

  /**
   * Funções de navegação
   */

  goToLandingPage() {
    this.router.navigate(['']);
  }

  navigateToForgotPassword() {
    this.router.navigate(['recovery']);
  }
}
