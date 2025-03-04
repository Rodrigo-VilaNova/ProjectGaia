import { Component } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
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
      password: ['', [Validators.required]]
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
      .subscribe({
        next: response => {
          console.log('Login successful. Token:', response.Token);

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
}

interface LoginResponse {
  Token: string;
}
