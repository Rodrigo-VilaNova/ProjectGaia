import { Component } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-invoices',
  templateUrl: './invoices.component.html',
  styleUrls: ['./invoices.component.css'],
  standalone: true,
  imports: [RouterModule, CommonModule]
})
export class InvoicesComponent {
  constructor(private router: Router, private http: HttpClient) { }

  goToDashboard() {
    this.router.navigate(['/dashboard']);
  }
}
