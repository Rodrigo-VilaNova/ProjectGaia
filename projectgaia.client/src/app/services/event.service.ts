import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Observable, forkJoin, map, switchMap } from 'rxjs';

export interface Event {
  id: number;
  name: string;
  description: string;
  date: Date;
  type: EventType;
}
export enum EventType {
  Payment = 0,
  Price = 1,
  Miscellaneous = 2,
}

@Injectable({
  providedIn: 'root'
})
export class EventService {
  private apiUrl = `${environment.apiUrl}/events`;

  constructor(private http: HttpClient) { }

  getEventIds(): Observable<number[]> {
    return this.http.get<number[]>(`${this.apiUrl}`);
  }

  // Fetch event details for given IDs and sort them
  getEventsByIds(eventIds: number[]): Observable<any[]> {
    if (eventIds.length === 0) return new Observable(observer => observer.next([]));

    const requests = eventIds.map(id => this.http.get<any>(`${this.apiUrl}/${id}`));
    return forkJoin(requests).pipe(
      map(events => events.sort((a, b) => a.id - b.id)) // Sort events by ID
    );
  }

  // Fetch all events for the current user
  getUserEvents(): Observable<any[]> {
    return this.getEventIds().pipe(
      switchMap(ids => this.getEventsByIds(ids)) // Use the IDs to fetch full events
    );
  }
}
