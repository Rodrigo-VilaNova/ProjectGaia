import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../interceptors/auth.service';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { NavbarComponent } from '../navbar/navbar.component';


@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.css'],
  standalone: true,
  imports: [NavbarComponent]
})
export class ProfileComponent {
  constructor(private router: Router, private http: HttpClient, private authService: AuthService) { }

  goBack() {
    this.router.navigate(['/dashboard']);
  }

  clearToken() {
    this.authService.removeToken();
    this.router.navigate(['']);
  }

  changePassword() {
    this.router.navigate(['/change-password']);
  }

  logoutAccount() {
    this.http.delete(`${environment.apiUrl}/account/logout`, { responseType: 'text' })
      .subscribe(
        () => {
          this.clearToken();
        },
        (error) => {
          console.error('Error logging out:', error);
          alert('Failed to logout. Please try again.');
        }
      );
  }

  deleteAccount() {
    if (confirm('Are you sure you want to delete your account? This action cannot be undone.')) {
      this.http.delete(`${environment.apiUrl}/account/delete`, { responseType: 'text' })
        .subscribe(
          () => {
            alert('Account deleted successfully.');
            this.clearToken();
          },
          (error) => {
            console.error('Error deleting account:', error);
            alert('Failed to delete account. Please try again.');
          }
        );
    }
  }
}
