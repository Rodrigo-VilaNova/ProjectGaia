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

  selectedDate: Date | null = null;

  events = [
    { date: new Date(2024, 2, 10), name: "Reunião de Equipa" },
    { date: new Date(2024, 2, 15), name: "Apresentação de Projeto" },
    { date: new Date(2024, 2, 20), name: "Entrega de Relatório" }
  ];

  constructor(private router: Router) { }

  onDateChange(newDate: Date) {
    this.selectedDate = newDate;
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
