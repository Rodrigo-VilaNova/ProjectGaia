import { Component, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { InvoiceService, Invoice } from '../invoice.service';

@Component({
  selector: 'app-invoices',
  templateUrl: './invoices.component.html',
  styleUrls: ['./invoices.component.css'],
  standalone: true,
  imports: [RouterModule, CommonModule]
})
export class InvoicesComponent {
  invoices: Invoice[] = [];
  constructor(private router: Router, private http: HttpClient, private invoiceService: InvoiceService) { }

  ngOnInit(): void {
    this.loadInvoices();
  }

  loadInvoices(): void {
    this.invoiceService.getInvoices().subscribe(
      (data) => {
        this.invoices = data;
      },
      (error) => {
        console.error('Error fetching invoices:', error);
      }
    );
  }

  goToDashboard() {
    this.router.navigate(['/dashboard']);
  }
}
