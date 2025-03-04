import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { CommonModule } from '@angular/common';


@Component({
  selector: 'app-registered',
  templateUrl: './registered.component.html',
  styleUrls: ['./registered.component.css'],
  imports: [CommonModule]
})
export class RegisteredComponent implements OnInit {

  message: string = '';
  isLoading: boolean = true;

  constructor(private router: Router, private http: HttpClient, private activatedRoute: ActivatedRoute) { }

  ngOnInit(): void {
    this.registerAccount();
  }

  registerAccount(): void {
    const token = this.activatedRoute.snapshot.queryParamMap.get('token');

    this.http.get(`${environment.apiUrl}/account/confirm?token=${token}`, { responseType: 'text' })
      .subscribe({
        next: (response: string) => {
          this.message = response ;
          this.isLoading = false;
        },
        error: (error) => {
          this.message = error.error || `An unexpected error occured with no message, error code ${error.status}`;
          this.isLoading = false;
        }
      });
  }

  returnToLanding(): void {
    this.router.navigate(['/landing']);
  }
}
