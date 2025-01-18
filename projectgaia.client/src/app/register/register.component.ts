import { Component } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css'],
  standalone: true,
  imports: [RouterModule, ReactiveFormsModule, CommonModule]
})
export class RegisterComponent {
  registerForm: FormGroup;
  loading = false;
  errorMessage: string | null = null;

  constructor(private fb: FormBuilder, private http: HttpClient, private router: Router) {
    this.registerForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(64)]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(8), Validators.maxLength(128)]],
      confirmPassword: ['', [Validators.required]]
    }, {
      //validators: this.passwordMatchValidator
    });
  }

  // Password match validator
  passwordMatchValidator(group: FormGroup): { [key: string]: boolean } | null {
    const password = group.get('password')?.value;
    const confirmPassword = group.get('confirmPassword')?.value;
    if (password && confirmPassword && password !== confirmPassword) {
      return { 'passwordMismatch': true };
    }
    return null;
  }

  onSubmit() {
    if (this.registerForm.invalid) {
      return;
    }

    this.loading = true;
    this.errorMessage = null;

    const accountDTO = {
      name: this.registerForm.value.name,
      email: this.registerForm.value.email,
      password: this.registerForm.value.password
    };

    this.http.post<RegisterResponse>('https://localhost:7277/account/register', accountDTO)
      .subscribe(
        (response) => {
          console.log('Registration successful. Token:', response.Token); // Log the token

          // Optionally store the token (e.g., in localStorage)
          localStorage.setItem('authToken', response.Token);

          // Redirect to the login page or another route
          this.router.navigate(['/dashboard']);
        },
        (error) => {
          console.error('Registration error:', error); // Log the error for debugging
          this.errorMessage = 'Registration failed. Please try again.';
          this.loading = false;
        }
      );

  }
}

interface RegisterResponse {
  Token: string;
}

