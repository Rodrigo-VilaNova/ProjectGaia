import { Component, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { EventService, Event } from '../services/event.service';
import { Invoice, InvoiceService } from '../services/invoice.service';
import { NavbarComponent } from '../navbar/navbar.component';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css'],
  standalone: true,
  imports: [RouterModule, CommonModule, NavbarComponent]
})

/**
  * Componente responsável pela dashboard/página principal
  */
export class DashboardComponent {

  /**
   * Construtor do componente
   * @param router Serviço de routing para navegação
   * @param eventService Serviço responsável pelos eventos
   * @param invoiceService Serviço responsável pelas faturas
   */
  constructor(private router: Router, private eventService: EventService, private invoiceService: InvoiceService) { }

  /** Eventos a mostrar na dashboard */
  dashboardEvents: Event[] = [];

  /** Faturas armazenadas usadas para cálculos */
  invoices: Invoice[] = [];

  /** Custo médio das faturas armazenadas */
  averagePrice: number = 0;

  /** Consumo médio das faturas armazenadas*/
  averageConsumption: number = 0;

  /**
   * Método do ciclo de vida do Angular chamado quando o componente é inicializado.
   * Carrega os eventos e faturas necessários
   */
  ngOnInit() {
    this.loadUpcomingEvents();
    this.loadInvoices();
  }

  /**
   * Carrega todos os eventos que acontecem dentro de uma semana
   */
  loadUpcomingEvents() {
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    const nextWeek = new Date();
    nextWeek.setDate(today.getDate() + 7);

    //Buscar todos os eventos que ocorrem dentro de uma semana
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

  /**
   * Carrega todas as faturas para calcular o custo e consumo médio
   */
  loadInvoices() {
    this.invoiceService.getUserInvoices().subscribe(
      (data) => {
        this.invoices = data;
        this.averagePrice = parseFloat((this.invoices.reduce((sum, invoice) => sum + invoice.price, 0) / this.invoices.length).toFixed(2));
        this.averageConsumption = parseFloat((this.invoices.reduce((sum, invoice) => sum + invoice.consumption, 0) / this.invoices.length).toFixed(2));
      },
      (error) => {
        console.error('Error fetching invoices:', error);
      }
    );
  }

  /**
   * Muda a classe css do elemento html que contém os valores do custo e consumo médio
   * @param value O custo ou consumo médio
   * @param limit Um limite para qualquer um dos valores anteriormente declarados
   * @returns Um representação string da classe do elemento html
   */
  getBoxClass(value: number, limit: number): string {
    if (value > limit) {
      return 'high-consumption'; 
    } else if (value >= limit * 0.8) {
      return 'warning-consumption'; 
    } else {
      return 'normal-consumption'; 
    }
  }
}
