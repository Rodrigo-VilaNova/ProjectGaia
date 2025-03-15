import { Component } from '@angular/core';
import { BsDatepickerModule } from 'ngx-bootstrap/datepicker';
import { Router, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../interceptors/auth.service';
import { EventService } from '../services/event.service';

@Component({
  selector: 'app-events',
  templateUrl: './events.component.html',
  styleUrls: ['./events.component.css'],
  standalone: true,
  imports: [RouterModule, CommonModule, BsDatepickerModule, FormsModule],
})

export class EventsComponent {

  selectedDate: string = '';
  filteredEvents: { id: number;  date: string; description: string; name: string, type: EventType }[] = [];
  selectedEvents: number[] = [];

  events = [
    { id: 1, date: '2025-01-10', description: "Tarefa muito atrasada", name: "Tarefa esquecida", type: EventType.Miscellaneous, },
    { id: 2, date: '2025-03-09', description: "Dia 9 de Março", name: "Dia 9/3", type: EventType.Miscellaneous, },
    { id: 3, date: '2025-03-10', description: "Tarifa Aumenta 3%", name: "Aumento Tarifa Eletricidade", type: EventType.Price, },
    { id: 4, date: '2025-03-10', description: "Pagar conta da eletricidade", name: "Conta Eletricidade", type: EventType.Payment, },
    { id: 5, date: '2025-03-15', description: "Possível redução de tarifa de 1%", name: "Redução Tarifa Eletricidade", type: EventType.Price, },
    { id: 6, date: '2025-03-20', description: "Pagar mensalidade do carro elétrico", name: "Mensalidade Carro Elétrico", type: EventType.Payment, },
  ];
  constructor(private router: Router) {
    this.filterEvents();
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

  isOverdue(eventDate: string): boolean {
    const eventDateObj = new Date(eventDate);
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    return eventDateObj < today;
  }

  toggleEventSelection(eventId: number, event: Event) {
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
    this.events = this.events.filter(event => !this.selectedEvents.includes(event.id));
    this.filterEvents();
    this.selectedEvents = [];
  }

  goToDashboard() {
    this.router.navigate(['/dashboard']);
  }

  goToInvoices() {
    this.router.navigate(['/invoices']);
  }

  goToProfile() {
    this.router.navigate(['/account']);
  }

  goToSimulation() {
    this.router.navigate(['/simulation']);
  }
}

export enum EventType {
  Payment = 'Payment',
  Price = 'Price',
  Miscellaneous = 'Miscellaneous',
}
