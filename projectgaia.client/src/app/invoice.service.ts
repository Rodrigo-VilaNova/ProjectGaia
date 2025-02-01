import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

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
  private apiUrl = 'https://localhost:7277/api/invoices';

  constructor(private http: HttpClient) { }

  getInvoices(): Observable<Invoice[]> {
    return this.http.get<Invoice[]>(this.apiUrl);
  }
}
