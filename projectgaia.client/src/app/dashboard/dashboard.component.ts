import { Component, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { AuthService } from '../interceptors/auth.service';
import { EventService, Event } from '../services/event.service';
import { Invoice, InvoiceService } from '../services/invoice.service';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css'],
  standalone: true,
  imports: [RouterModule, CommonModule]
})
export class DashboardComponent {
  constructor(private router: Router, private eventService: EventService, private invoiceService: InvoiceService) { }

  dashboardEvents: Event[] = [];
  invoices: Invoice[] = [];

  averagePrice: number = 0;
  averageConsumption: number = 0;

  ngOnInit() {
    this.loadUpcomingEvents();
    this.loadInvoices();
  }



  //Carrega eventos que occorrem dentro de uma semana
  loadUpcomingEvents() {
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    const nextWeek = new Date();
    nextWeek.setDate(today.getDate() + 7);

    this.eventService.getUserEvents().subscribe(
      (data) => {
        this.dashboardEvents = data
          .filter(event => {
            const eventDate = new Date(event.date);
            return eventDate >= today && eventDate <= nextWeek;
          })
          .sort((a, b) => new Date(a.date).getTime() - new Date(b.date).getTime());
      },
      (error) => {
        console.error('Error fetching events:', error);
      }
    );
  }

  loadInvoices() {
    this.invoiceService.getUserInvoices().subscribe(
      (data) => {
        this.invoices = data;
        this.averagePrice = this.invoices.reduce((sum, invoice) => sum + invoice.price, 0) / this.invoices.length;
        this.averageConsumption = this.invoices.reduce((sum, invoice) => sum + invoice.consumption, 0) / this.invoices.length;
      },
      (error) => {
        console.error('Error fetching invoices:', error);
      }
    );
  }


  //Funções de routing
  goToInvoices() {
    this.router.navigate(['/invoices']);
  }

  goToProfile() {
    this.router.navigate(['/account']);
  }

  goToEvents() {
    this.router.navigate(['/events']);
  }

  goToSimulation() {
    this.router.navigate(['/simulation']);
  }
}
