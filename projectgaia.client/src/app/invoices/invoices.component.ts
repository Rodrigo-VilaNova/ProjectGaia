import { Component, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { InvoiceService, Invoice } from '../services/invoice.service';
import { environment } from '../../environments/environment';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-invoices',
  templateUrl: './invoices.component.html',
  styleUrls: ['./invoices.component.css'],
  standalone: true,
  imports: [RouterModule, CommonModule, FormsModule]
})
export class InvoicesComponent implements OnInit {
  invoices: Invoice[] = [];
  filteredInvoices: Invoice[] = [];
  selectedInvoices: number[] = [];
  showFilterOverlay = false;

  filterOptions = {
    price: { min: null, max: null, order: null },
    consumption: { min: null, max: null, order: null },
    emissionDate: { order: null },
    uploadDate: { order: null }
  };

  filters = {
    idOrder: '', // 'asc' or 'desc'
    priceMin: null,
    priceMax: null,
    consumptionMin: null,
    consumptionMax: null,
    emissionDateMin: '',
    emissionDateMax: '',
    uploadDateMin: '',
    uploadDateMax: ''
  };

  constructor(private router: Router, private http: HttpClient, private invoiceService: InvoiceService) { }

  ngOnInit() {
    this.loadInvoices();
  }

  goToProfile() {
    this.router.navigate(['/account']);
  }

  toggleFilterOverlay() {
    this.showFilterOverlay = !this.showFilterOverlay;
  }

  loadInvoices() {
    this.invoiceService.getUserInvoices().subscribe(
      (data) => {
        this.invoices = data;
        this.applyFilters();
      },
      (error) => {
        console.error('Error fetching invoices:', error);
      }
    );
  }

  applyFilters() {
    this.filteredInvoices = [...this.invoices];

    // Apply min/max filters
    this.filteredInvoices = this.filteredInvoices.filter(invoice =>
      (this.filterOptions.price.min == null || invoice.price >= this.filterOptions.price.min) &&
      (this.filterOptions.price.max == null || invoice.price <= this.filterOptions.price.max) &&
      (this.filterOptions.consumption.min == null || invoice.consumption >= this.filterOptions.consumption.min) &&
      (this.filterOptions.consumption.max == null || invoice.consumption <= this.filterOptions.consumption.max)
    );

    // Apply sorting
    Object.keys(this.filterOptions).forEach(key => {
      const typedKey = key as keyof Invoice; // Explicitly cast key
      const { order } = this.filterOptions[typedKey as keyof typeof this.filterOptions];

      if (order) {
        this.filteredInvoices.sort((a, b) =>
          order === 'asc' ? (a[typedKey] > b[typedKey] ? 1 : -1) : (a[typedKey] < b[typedKey] ? 1 : -1)
        );
      }
    });
  }

  resetFilters() {
    this.filterOptions = {
      price: { min: null, max: null, order: null },
      consumption: { min: null, max: null, order: null },
      emissionDate: { order: null },
      uploadDate: { order: null }
    };
    this.applyFilters();
  }

  toggleInvoiceSelection(invoiceId: number, event: globalThis.Event) {
    const isChecked = (event.target as HTMLInputElement).checked;
    if (isChecked) {
      this.selectedInvoices.push(invoiceId);
    } else {
      this.selectedInvoices = this.selectedInvoices.filter(id => id !== invoiceId);
    }
  }

  deleteSelectedInvoices() {
    if (this.selectedInvoices.length === 0) return;
    if (!confirm("Are you sure you wish to delete selected invoices?")) return;

    const deleteRequests = this.selectedInvoices.map(id =>
      this.http.delete(`${environment.apiUrl}/invoices/${id}`).toPromise()
    );

    Promise.all(deleteRequests)
      .then(() => {
        this.invoices = this.invoices.filter(invoice => !this.selectedInvoices.includes(invoice.id));
        this.selectedInvoices = [];
      })
      .catch(error => {
        console.error("Error deleting invoices:", error);
      })
      .finally(() => { window.location.reload(); });
  }

  goToAddInvoice() {
    this.router.navigate(['/add-invoice']);
  }

  goToDashboard() {
    this.router.navigate(['/dashboard']);
  }

  goToEvents() {
    this.router.navigate(['/events']);
  }

  goToSimulation() {
    this.router.navigate(['/simulation']);
  }
}
