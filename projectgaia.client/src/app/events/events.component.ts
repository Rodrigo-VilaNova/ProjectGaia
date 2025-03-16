import { Component } from '@angular/core';

import { Router, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../interceptors/auth.service';
import { EventService, Event, EventType } from '../services/event.service';
import { environment } from '../../environments/environment';

@Component({
  selector: 'app-events',
  templateUrl: './events.component.html',
  styleUrls: ['./events.component.css'],
  standalone: true,
  imports: [RouterModule, CommonModule, FormsModule]
})

export class EventsComponent {

  selectedDate: Date | null = null;
  filteredEvents: Event[] = [];
  selectedEvents: number[] = [];

  events: Event[] = [];

  currentSortColumn: string | null = null;
  currentSortOrder: 'asc' | 'desc' | null = null;

  /*events = [
    { id: 1, date: '2025-01-10', description: "Tarefa muito atrasada", name: "Tarefa esquecida", type: EventType.Miscellaneous, },
    { id: 2, date: '2025-03-09', description: "Dia 9 de Março", name: "Dia 9/3", type: EventType.Miscellaneous, },
    { id: 3, date: '2025-03-10', description: "Tarifa Aumenta 3%", name: "Aumento Tarifa Eletricidade", type: EventType.Price, },
    { id: 4, date: '2025-03-10', description: "Pagar conta da eletricidade", name: "Conta Eletricidade", type: EventType.Payment, },
    { id: 5, date: '2025-03-15', description: "Possível redução de tarifa de 1%", name: "Redução Tarifa Eletricidade", type: EventType.Price, },
    { id: 6, date: '2025-03-20', description: "Pagar mensalidade do carro elétrico", name: "Mensalidade Carro Elétrico", type: EventType.Payment, },
  ];*/

  constructor(private router: Router, private http: HttpClient, private eventService: EventService) {
    this.filterEvents();
  }

  ngOnInit() {
    this.loadEvents();
  }

  loadEvents() {
    this.eventService.getUserEvents().subscribe(
      (data) => {
        this.events = data;
        this.filterEvents();
      },
      (error) => {
        console.error('Error fetching invoices:', error);
      }
    );
  }

  onDateChange(event: any) {
    this.selectedDate = event.target.value;
    this.filterEvents();
  }

  filterEvents() {
    if (!this.selectedDate) {
      this.filteredEvents = [...this.events];
      return;
    }
    this.filteredEvents = this.events.filter(event => event.date === this.selectedDate);
  }

  isOverdue(eventDate: Date): boolean {
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    return eventDate < today;
  }

  toggleEventSelection(eventId: number, event: globalThis.Event) {
    const isChecked = (event.target as HTMLInputElement).checked;
    if (isChecked) {
      this.selectedEvents.push(eventId);
    } else {
      this.selectedEvents = this.selectedEvents.filter(id => id !== eventId);
    }
  }

  deleteSelectedEvents() {
    if (this.selectedEvents.length === 0) return;
    if (!confirm("Are you sure you wish to delete selected events?")) return;

    const deleteRequests = this.selectedEvents.map(id =>
      this.http.delete(`${environment.apiUrl}/events/${id}`).toPromise()
    );

    Promise.all(deleteRequests)
      .then(() => {
        this.events = this.events.filter(event => !this.selectedEvents.includes(event.id));
        this.filterEvents();
        this.selectedEvents = [];
      })
      .catch(error => {
        console.error("Error deleting invoices:", error);
      })
      .finally(() => { window.location.reload(); });
  }

  sortBy(column: keyof Event) {
    if (this.currentSortColumn === column) {
      this.currentSortOrder = this.currentSortOrder === 'asc' ? 'desc' : 'asc';
    } else {
      this.currentSortColumn = column;
      this.currentSortOrder = 'asc';
    }

    this.filteredEvents.sort((a, b) => {
      if (a[column] < b[column]) return this.currentSortOrder === 'asc' ? -1 : 1;
      if (a[column] > b[column]) return this.currentSortOrder === 'asc' ? 1 : -1;
      return 0;
    });
  }

  getEventTypeName(type: EventType): string {
    return EventType[type];
  }

  goToDashboard() {
    this.router.navigate(['/dashboard']);
  }

  goToInvoices() {
    this.router.navigate(['/invoices']);
  }

  goToAddEvent() {
    this.router.navigate(['/add-event']);
  }

  goToProfile() {
    this.router.navigate(['/account']);
  }

  goToSimulation() {
    this.router.navigate(['/simulation']);
  }
}
