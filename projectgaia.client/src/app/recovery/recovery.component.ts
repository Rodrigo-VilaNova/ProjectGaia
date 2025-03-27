import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { environment } from '../../environments/environment';
import { CommonModule } from '@angular/common';
@Component({
  selector: 'app-recovery',
  templateUrl: './recovery.component.html',
  styleUrls: ['./recovery.component.css'],
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule],
})
export class RecoveryComponent {
  emailForm: FormGroup;
  loading = false;
  serverResponse: string | null = null;
  serverError: string | null = null;

  constructor(private fb: FormBuilder, private http: HttpClient, private router: Router) {
    this.emailForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]]
    });
  }

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

  goToLandingPage() {
    this.router.navigate(['/']);
  }
}
