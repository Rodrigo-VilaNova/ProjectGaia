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
  selectedInvoices: number[] = [];
  constructor(private router: Router, private http: HttpClient, private invoiceService: InvoiceService) { }

  ngOnInit() {
    this.loadInvoices();
  }

  loadInvoices() {
    this.invoiceService.getUserInvoices().subscribe(
      (sortedInvoices) => {
        this.invoices = sortedInvoices;
        console.log('Invoices fetched and sorted:', this.invoices);
      },
      (error) => {
        console.error('Error fetching invoices:', error);
      }
    );
  }

  toggleInvoiceSelection(invoiceId: number, event: Event) {
    const isChecked = (event.target as HTMLInputElement).checked;
    if (isChecked) {
      this.selectedInvoices.push(invoiceId);
    } else {
      this.selectedInvoices = this.selectedInvoices.filter(id => id !== invoiceId);
    }
  }

  deleteSelectedInvoices() {
    if (this.selectedInvoices.length === 0) {
      return;
    }

    const confirmDelete = confirm("Are you sure you wish to delete selected invoices?");
    if (!confirmDelete) return;

    const deleteRequests = this.selectedInvoices.map(id =>
      this.http.delete(`https://localhost:7277/api/invoices/${id}`).toPromise()
    );

    Promise.all(deleteRequests)
      .then(() => {
        this.invoices = this.invoices.filter(invoice => !this.selectedInvoices.includes(invoice.id));
        this.selectedInvoices = [];
      })
      .catch(error => {
        console.error("Erro ao apagar faturas", error);
      })
      .finally(() => { window.location.reload(); }
      );
  }

  getInvoices() {
    this.http.get<Invoice[]>('https://localhost:7277/api/invoices').subscribe(
      (data) => {
        this.invoices = data;
      },
      (error) => console.error("Erro ao carregar faturas", error)
    );
  }

  goToAddInvoice() {
    this.router.navigate(['/add-invoice']);
  }

  goToDashboard() {
    this.router.navigate(['/dashboard']);
  }
}
