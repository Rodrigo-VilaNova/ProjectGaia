import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors, ReactiveFormsModule } from '@angular/forms';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { environment } from '../../environments/environment';
import { CommonModule } from '@angular/common';
import { NavbarComponent } from '../navbar/navbar.component';

@Component({
  selector: 'app-change-password',
  templateUrl: './change-password.component.html',
  styleUrls: ['./change-password.component.css'],
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule, NavbarComponent]
})
export class ChangePasswordComponent {
  changePasswordForm: FormGroup;
  loading = false;
  errorMessage: string | null = null;
  successMessage: string | null = null;

  constructor(private fb: FormBuilder, private http: HttpClient, private router: Router) {
    this.changePasswordForm = this.fb.group({
      currentPassword: ['', [Validators.required]],
      newPassword: ['', [Validators.required, Validators.minLength(8), Validators.maxLength(128), this.passwordStrengthValidator]],
      confirmPassword: ['', Validators.required]
    }, {
      validators: this.passwordMatchValidator
    });
  }

  passwordMatchValidator(group: AbstractControl): ValidationErrors | null {
    const password = group.get('newPassword')?.value;
    const confirmPassword = group.get('confirmPassword')?.value;
    return password === confirmPassword ? null : { passwordMismatch: true };
  }

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

  goBack() {
    this.router.navigate(['/account']);
  }

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
}
