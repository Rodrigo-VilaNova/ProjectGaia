import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, forkJoin } from 'rxjs';
import { map, switchMap } from 'rxjs/operators';
import { environment } from '../../environments/environment';

/**
 * Interface que representa uma fatura e respetivos dados
 */
export interface Invoice {
  id: number;
  price: number;
  consumption: number;
  emissionDate: Date;
  uploadDate: Date;
  accountID: number;
}

@Injectable({
  providedIn: 'root'
})

/**
 * Serviço responsável pelo tratamento das faturas e comunicação com o backend
 */
export class InvoiceService {
  // O URL da API
  private apiUrl = `${environment.apiUrl}/invoices`;

  /**
   * Construtor do componente
   * @param http Cliente HTTP para comunicação com a API
   */
  constructor(private http: HttpClient) { }

  /**
   * Retorna os IDs de todos as faturas
   * @returns Uma lista dos IDs das faturas
   */
  getInvoiceIds(): Observable<number[]> {
    return this.http.get<number[]>(`${this.apiUrl}`);
  }

  /**
   * Retorna os detalhes das faturas através dos respetivos IDs
   * @param invoiceIds Os IDs das faturas
   * @returns Observable das faturas e respetivos dados
   */
  getInvoicesByIds(invoiceIds: number[]): Observable<any[]> {
    if (invoiceIds.length === 0) return new Observable(observer => observer.next([]));

    const requests = invoiceIds.map(id => this.http.get<any>(`${this.apiUrl}/${id}`));
    return forkJoin(requests).pipe(
      map(invoices => invoices.sort((a, b) => a.id - b.id))
    );
  }

  /**
   * Retorna todos as faturas do utilizador atual
   * @returns Observable das faturas do utilizador
   */
  getUserInvoices(): Observable<any[]> {
    return this.getInvoiceIds().pipe(
      switchMap(ids => this.getInvoicesByIds(ids))
    );
  }
}
