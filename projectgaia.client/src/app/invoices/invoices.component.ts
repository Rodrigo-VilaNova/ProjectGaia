import { Component, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { InvoiceService, Invoice } from '../invoice.service';
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

  currentSortColumn: string | null = null;
  currentSortOrder: 'asc' | 'desc' | null = null;

  static readonly FILTERS_DEFAULT = {
    id: { min: null as number | null, max: null as number | null },
    price: { min: null as number | null, max: null as number | null },
    consumption: { min: null as number | null, max: null as number | null },
    emissionDate: { min: null as Date | null, max: null as Date | null },
    uploadDate: { min: null as Date | null, max: null as Date | null }
  };

  filters = structuredClone(InvoicesComponent.FILTERS_DEFAULT);

  hasFiltersApplied = this.isFilterModified();

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

  isFilterModified(): boolean {
  // Loop through each key in the filters object
  for (let key in this.filters) {
    const filter = this.filters[key as keyof typeof this.filters];
    const default_filter = InvoicesComponent.FILTERS_DEFAULT[key as keyof typeof InvoicesComponent.FILTERS_DEFAULT];

    // Check if the current filter's min/max value differs from the default (which is null)
    if (filter.min !== default_filter.min ||
      filter.max !== default_filter.max) {
      return true;
    }
  }

  // If no filter is modified, return false
  return false;
}

  applyFilters() {
    this.hasFiltersApplied = this.isFilterModified();

    this.filteredInvoices = [...this.invoices];

    // Apply min/max filters
    this.filteredInvoices = this.filteredInvoices.filter(invoice =>
      (this.filters.id.min == null || invoice.id >= this.filters.id.min) &&
      (this.filters.id.max == null || invoice.id <= this.filters.id.max) &&
      (this.filters.price.min == null || invoice.price >= this.filters.price.min) &&
      (this.filters.price.max == null || invoice.price <= this.filters.price.max) &&
      (this.filters.consumption.min == null || invoice.consumption >= this.filters.consumption.min) &&
      (this.filters.consumption.max == null || invoice.consumption <= this.filters.consumption.max) &&
      (this.filters.emissionDate.min == null || invoice.emissionDate >= this.filters.emissionDate.min) &&
      (this.filters.emissionDate.max == null || invoice.emissionDate <= this.filters.emissionDate.max) &&
      (this.filters.uploadDate.min == null || invoice.emissionDate >= this.filters.uploadDate.min) &&
      (this.filters.uploadDate.max == null || invoice.emissionDate <= this.filters.uploadDate.max)
    );
  }

  resetFilters() {
    this.filters = structuredClone(InvoicesComponent.FILTERS_DEFAULT);
    this.applyFilters();
  }

  toggleInvoiceSelection(invoiceId: number, event: Event) {
    const isChecked = (event.target as HTMLInputElement).checked;
    if (isChecked) {
      this.selectedInvoices.push(invoiceId);
    } else {
      this.selectedInvoices = this.selectedInvoices.filter(id => id !== invoiceId);
    }
    this.applyFilters(); // Update table to show only selected invoices
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

  sortBy(column: keyof Invoice) {
    if (this.currentSortColumn === column) {
      this.currentSortOrder = this.currentSortOrder === 'asc' ? 'desc' : 'asc';
    } else {
      this.currentSortColumn = column;
      this.currentSortOrder = 'asc';
    }

    this.filteredInvoices.sort((a, b) => {
      if (a[column] < b[column]) return this.currentSortOrder === 'asc' ? -1 : 1;
      if (a[column] > b[column]) return this.currentSortOrder === 'asc' ? 1 : -1;
      return 0;
    });
  }

  goToAddInvoice() {
    this.router.navigate(['/add-invoice']);
  }

  goToDashboard() {
    this.router.navigate(['/dashboard']);
  }
}
