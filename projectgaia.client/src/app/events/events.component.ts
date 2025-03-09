import { Component } from '@angular/core';
import { BsDatepickerModule } from 'ngx-bootstrap/datepicker';
import { Router, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../interceptors/auth.service';

@Component({
  selector: 'app-events',
  templateUrl: './events.component.html',
  styleUrls: ['./events.component.css'],
  standalone: true,
  imports: [RouterModule, CommonModule, BsDatepickerModule, FormsModule],
})

export class EventsComponent {

  placeholderDate = new Date();
  selectedDate: string = '';
  filteredEvents: { date: string; description: string; name: string }[] = [];

  events = [
    { date: '2025-01-10', description: "Tarefa muito atrasada", name: "Tarefa esquecida" },
    { date: '2025-03-09', description: "Dia 9 de Março", name: "Dia 9/3" },
    { date: '2025-03-10', description: "Reunião da DevTeam", name: "Reunião de Equipa" },
    { date: '2025-03-10', description: "Apresentação de Project Gaia", name: "Apresentação de Projeto" },
    { date: '2025-03-15', description: "Entrega do Projeto", name: "Entrega de Relatório" },
    { date: '2025-03-20', description: "Check-in com o Cliente às 16h00", name: "Check-in com o Cliente" }
  ];

  constructor(private router: Router) {
    this.setTodayAsDefault();
  }

  setTodayAsDefault() {
    const today = new Date();
    this.selectedDate = today.toISOString().split('T')[0];
    this.filterEvents();
  }

  isOverdue(eventDate: string): boolean {
    const eventDateObj = new Date(eventDate);
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    return eventDateObj < today;
  }

  filterEvents() {
    if (!this.selectedDate) {
      this.filteredEvents = [];
      return;
    }
    this.filteredEvents = this.events.filter(event => event.date === this.selectedDate);
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
