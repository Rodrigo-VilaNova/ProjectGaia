import { Component } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { AuthService } from '../interceptors/auth.service';
import { environment } from '../../environments/environment';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
  standalone: true,
  imports: [RouterModule, ReactiveFormsModule, CommonModule]
})
export class LoginComponent {
  loginForm: FormGroup;
  loading = false;
  errorMessage: string | null = null;

  constructor(private fb: FormBuilder, private http: HttpClient, private router: Router, private authService: AuthService) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(8), Validators.maxLength(128)]]
    });
  }

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
      .subscribe(
        (response) => {
          console.log('Login successful. Token:', response.Token); // Log the token

          // Store the token (e.g., in localStorage)
          localStorage.setItem('authToken', response.Token);
          this.authService.setToken(response.Token);

          // Redirect to the home page or another route
          this.router.navigate(['/dashboard']);
        },
        (error) => {
          console.error('Login error:', error); // Log the error for debugging
          this.errorMessage = 'Login failed. Please check your credentials and try again.';
          this.loading = false;
        }
      );
  }
}

interface LoginResponse {
  Token: string;
}
