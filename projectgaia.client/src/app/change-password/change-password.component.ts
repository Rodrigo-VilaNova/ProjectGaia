import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors, ReactiveFormsModule } from '@angular/forms';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { environment } from '../../environments/environment';
import { CommonModule } from '@angular/common';
import { NavbarComponent } from '../navbar/navbar.component';
import { FooterComponent } from '../footer/footer.component';

@Component({
  selector: 'app-change-password',
  templateUrl: './change-password.component.html',
  styleUrls: ['./change-password.component.css'],
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule, NavbarComponent, FooterComponent]
})

/**
  * Componente responsável pela mudança da password do utilizador.
  */
export class ChangePasswordComponent {

  /** Formulário para a mudança da password */
  changePasswordForm: FormGroup;

  /** Indica se o formulário está em processo de submissão para evitar múltiplos envios */
  loading = false;

  /** Mensagem de erro exibida ao utilizador */
  errorMessage: string | null = null;

  /** Mensagem de sucesso exibida ao utilizador */
  successMessage: string | null = null;

  /**
   * Construtor do componente
   * @param fb FormBuilder para criação e validação do formulário
   * @param http Serviço HTTP para comunicação com a API
   * @param router Serviço de routing para navegação
   */
  constructor(private fb: FormBuilder, private http: HttpClient, private router: Router) {
    //Criação do formulário com validações
    this.changePasswordForm = this.fb.group({
      currentPassword: ['', [Validators.required]],
      newPassword: ['', [Validators.required, Validators.minLength(8), Validators.maxLength(128), this.passwordStrengthValidator]],
      confirmPassword: ['', Validators.required]
    }, {
      validators: this.passwordMatchValidator
    });
  }

  /**
   * Verifica se a password inserida como confirmação coincide com a password inserida anteriormente
   * @param group Grupo de campos do formulário
   * @returns Um erro de validação se as passwords não forem iguais
   */
  passwordMatchValidator(group: AbstractControl): ValidationErrors | null {
    const password = group.get('newPassword')?.value;
    const confirmPassword = group.get('confirmPassword')?.value;
    return password === confirmPassword ? null : { passwordMismatch: true };
  }

  /**
   * Verifica se a password inserida é segura
   * @param control O campo associado á password inserida
   * @returns Um erro de validação caso a password não possua os requisitos
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
   * Submete o pedido de alteração de password após validações
   * @returns
   */
  onSubmit() {
    if (this.changePasswordForm.invalid) {
      this.errorMessage = 'Please correct the errors in the form.';
      return;
    }

    this.loading = true;
    this.errorMessage = null;
    this.successMessage = null;

    const passwordDTO = {
      oldPassword: this.changePasswordForm.value.currentPassword,
      newPassword: this.changePasswordForm.value.newPassword
    };

    this.http.put(`${environment.apiUrl}/account/password`, passwordDTO, { responseType: 'text' })
      .subscribe({
        next: (response: string) => {
          alert(response);
          this.router.navigate(['/account']);
        },
        error: (error: HttpErrorResponse) => {
          this.loading = false;

          this.errorMessage = error.error || `An unexpected error occured with no message, error code ${error.status}`;
          console.log('Error Status Code:', error.status, 'Response:', error.error);
        }
      });
  }

  /**
   * Redireciona o utilizador para a página da conta
   */
  goToAccount() {
    this.router.navigate(['/account']);
  }
}
