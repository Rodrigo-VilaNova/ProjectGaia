import { Component, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { AuthService } from '../interceptors/auth.service';
import { EventService, Event } from '../services/event.service';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css'],
  standalone: true,
  imports: [RouterModule, CommonModule]
})
export class DashboardComponent {
  constructor(private router: Router, private eventService: EventService) { }

  dashboardEvents: Event[] = [];

  ngOnInit() {
    this.loadUpcomingEvents();
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
