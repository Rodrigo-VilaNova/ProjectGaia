import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Observable, forkJoin, map, switchMap } from 'rxjs';

/**
 * Interface que representa um evento e respetivos dados
 */
export interface Event {
  id: number;
  name: string;
  description: string;
  date: Date;
  type: EventType;
}

/**
 * Interface que representa um tipo de evento
 */
export enum EventType {
  Payment = 0,
  Price = 1,
  Miscellaneous = 2,
}

@Injectable({
  providedIn: 'root'
})

/**
 * Serviço responsável pelo tratamento dos eventos e comunicação com o backend
 */
export class EventService {
  // O URL da API 
  private apiUrl = `${environment.apiUrl}/events`;

  /**
   * Construtor do componente
   * @param http Cliente HTTP para comunicação com a API
   */
  constructor(private http: HttpClient) { }

  /**
   * Retorna os IDs de todos os eventos
   * @returns Uma lista dos IDs dos eventos
   */
  getEventIds(): Observable<number[]> {
    return this.http.get<number[]>(`${this.apiUrl}`);
  }

  /**
   * Retorna os detalhes dos eventos através dos respetivos IDs
   * @param eventIds Os IDs dos eventos
   * @returns Observable dos eventos e respetivos dados
   */
  getEventsByIds(eventIds: number[]): Observable<any[]> {
    if (eventIds.length === 0) return new Observable(observer => observer.next([]));

    const requests = eventIds.map(id => this.http.get<any>(`${this.apiUrl}/${id}`));
    return forkJoin(requests).pipe(
      map(events => events.sort((a, b) => a.id - b.id))
    );
  }

  /**
   * Retorna todos os eventos do utilizador atual
   * @returns Observable dos eventos do utilizador
   */
  getUserEvents(): Observable<any[]> {
    return this.getEventIds().pipe(
      switchMap(ids => this.getEventsByIds(ids))
    );
  }
}
