import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors, ReactiveFormsModule } from '@angular/forms';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { ActivatedRoute, Router } from '@angular/router';
import { environment } from '../../environments/environment';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-reset-password',
  templateUrl: './reset-password.component.html',
  styleUrls: ['./reset-password.component.css'],
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule]
})
export class ResetPasswordComponent {
  resetPasswordForm: FormGroup;
  loading = false;
  errorMessage: string | null = null;
  successMessage: string | null = null;

  constructor(private fb: FormBuilder, private http: HttpClient, private router: Router, private activatedRoute: ActivatedRoute) {
    this.resetPasswordForm = this.fb.group({
      password: ['', [Validators.required, Validators.minLength(8), Validators.maxLength(128), this.passwordStrengthValidator]],
      confirmPassword: ['', Validators.required]
    }, {
      validators: this.passwordMatchValidator
    });
  }

  passwordMatchValidator(group: AbstractControl): ValidationErrors | null {
    const password = group.get('password')?.value;
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
