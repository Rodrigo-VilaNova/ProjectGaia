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

  selectedDate: string = '';
  filteredEvents: { date: string; description: string; name: string }[] = [];

  events = [
    { date: '2025-03-10', description: "Reunião da DevTeam", name: "Reunião de Equipa" },
    { date: '2025-03-10', description: "Apresentação de Project Gaia", name: "Apresentação de Projeto" },
    { date: '2025-03-15', description: "Entrega do Projeto", name: "Entrega de Relatório" },
    { date: '2025-03-20', description: "Check-in com o Cliente às 16h00", name: "Check-in com o Cliente" }
  ];

  constructor(private router: Router) { }

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
