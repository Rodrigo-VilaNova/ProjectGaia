import { Component } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css'],
  standalone: true,
  imports: [RouterModule, CommonModule]
})
export class DashboardComponent {
  constructor(private router: Router, private http: HttpClient) { }

  logout() {
    localStorage.removeItem('authToken');
    this.router.navigate(['']);
  }

  deleteAccount() {
    if (confirm('Are you sure you want to delete your account? This action cannot be undone.')) {
      this.http.delete('https://localhost:7277/account/delete', {
        headers: { Authorization: `Bearer ${localStorage.getItem('authToken')}` }, responseType: 'text'
      }).subscribe(
        () => {
          alert('Account deleted successfully.');
          this.logout();
        },
        (error) => {
          console.error('Error deleting account:', error);
          alert('Failed to delete account. Please try again.');
        }
      );
    }
  }
}
