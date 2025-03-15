import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, forkJoin } from 'rxjs';
import { map, switchMap } from 'rxjs/operators';
import { environment } from '../../environments/environment';

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
  private apiUrl = `${environment.apiUrl}/invoices`;

  constructor(private http: HttpClient) { }

  getInvoiceIds(): Observable<number[]> {
    return this.http.get<number[]>(`${this.apiUrl}`);
  }

  // Fetch invoice details for given IDs and sort them
  getInvoicesByIds(invoiceIds: number[]): Observable<any[]> {
    if (invoiceIds.length === 0) return new Observable(observer => observer.next([]));

    const requests = invoiceIds.map(id => this.http.get<any>(`${this.apiUrl}/${id}`));
    return forkJoin(requests).pipe(
      map(invoices => invoices.sort((a, b) => a.id - b.id)) // Sort invoices by ID
    );
  }

  // Fetch all invoices for the current user
  getUserInvoices(): Observable<any[]> {
    return this.getInvoiceIds().pipe(
      switchMap(ids => this.getInvoicesByIds(ids)) // Use the IDs to fetch full invoices
    );
  }
}
