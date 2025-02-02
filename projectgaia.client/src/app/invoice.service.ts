import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, forkJoin } from 'rxjs';
import { map } from 'rxjs/operators';

export interface Invoice {
  id: number;
  price: number;
  consumption: number;
  emissionDate: string;
  uploadDate: string;
  accountID: number;
}

@Injectable({
  providedIn: 'root'
})
export class InvoiceService {
  private apiUrl = 'https://localhost:7277/api/invoices/';

  constructor(private http: HttpClient) { }

  getInvoices(invoiceIds: number[]): Observable<any[]> {
    // Create an array of HTTP GET observables
    const requests = invoiceIds.map(id => this.http.get<any>(`${this.apiUrl}/${id}`));

    // Use forkJoin to make all requests in parallel, then sort the results
    return forkJoin(requests).pipe(
      map(invoices => invoices.sort((a, b) => a.id - b.id)) // Sort by ID
    );
  }
}
