import { Component } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EventService, Event, EventType } from '../services/event.service';
import { environment } from '../../environments/environment';
import { NavbarComponent } from '../navbar/navbar.component';
import { FooterComponent } from '../footer/footer.component';

@Component({
  selector: 'app-events',
  templateUrl: './events.component.html',
  styleUrls: ['./events.component.css'],
  standalone: true,
  imports: [RouterModule, CommonModule, FormsModule, NavbarComponent, FooterComponent]
})

/**
 * Componente responsável pelos eventos
 */
export class EventsComponent {

  /** Data selecionada no campo de pesquisa */
  selectedDate: Date | null = null;

  /** Lista dos eventos filtrados */
  filteredEvents: Event[] = [];

  /** Lista dos eventos selecionados */
  selectedEvents: number[] = [];

  /** Lista dos eventos */
  events: Event[] = [];

  /** Coluna a ser filtrada */
  currentSortColumn: string | null = null;

  /** Ordem do filtro */
  currentSortOrder: 'asc' | 'desc' | null = null;

  /**
   * Construtor do componente
   * @param router Serviço de routing para navegação
   * @param http Cliente HTTP para comunicação com a API
   * @param eventService Serviço responsável pelos eventos
   */
  constructor(private router: Router, private http: HttpClient, private eventService: EventService) {
    this.filterEvents();
  }

  /**
   * Método do ciclo de vida do Angular chamado quando o componente é inicializado.
   * Carrega os eventos na base de dados.
   */
  ngOnInit() {
    this.loadEvents();
  }

  /**
   * Busca todos os eventos associados ao ID do utilizador respetivo
   */
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

  /**
   * Mostra os eventos que existem na data escolhida
   * @returns A lista de eventos que ocorrem na data selecionada
   */
  filterEvents() {
    if (!this.selectedDate) {
      this.filteredEvents = [...this.events];
      return;
    }

    const selectedDate = new Date(this.selectedDate);
    selectedDate.setHours(0, 0, 0, 0); // Apenas considera o dia

    this.filteredEvents = this.events.filter(event => {
      const eventDate = new Date(event.date);
      eventDate.setHours(0, 0, 0, 0); // Apenas considera o dia

      return eventDate.getTime() === selectedDate.getTime();
    });
  }

  /**
   * Averigua se a data de um evento já passou
   * @param eventDate A data do evento
   * @returns True se a data for anterior ao dia atual, False caso contrário
   */
  isOverdue(eventDate: Date): boolean {
    const formattedDate = new Date(eventDate).toISOString().split('T')[0];
    return formattedDate < new Date().toISOString().split('T')[0];
  }

  /**
   * Permite a seleção de um evento
   * @param eventId O ID do evento selecionado
   * @param event O evento de dar check á checkbox
   */
  toggleEventSelection(eventId: number, event: globalThis.Event) {
    const isChecked = (event.target as HTMLInputElement).checked;
    if (isChecked) {
      this.selectedEvents.push(eventId);
    } else {
      this.selectedEvents = this.selectedEvents.filter(id => id !== eventId);
    }
  }

  /**
   * Apaga os eventos selecionados
   * @returns A lista de eventos sem os eventos selecionados anteriormente
   */
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

  /**
   * Filtra os eventos de acordo com o especificado
   * @param column A coluna a filtrar por ordem crescente ou decrescente
   */
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

  /**
   * Retorna uma representação string do tipo do evento
   * @param type O tipo do evento
   * @returns A representação string desse tipo de evento
   */
  getEventTypeName(type: EventType): string {
    return EventType[type];
  }

  /**
   * Navega para a página de edição caso apenas 1 evento seja selecionado
   * @returns Uma mensagem de erro caso mais de 1 evento seja selecionado
   */
  editSelectedEvent() {
    if (this.selectedEvents.length !== 1) {
      alert("Please select exactly one event to edit.");
      return;
    }

    const eventId = this.selectedEvents[0];
    this.router.navigate([`/edit-event/${eventId}`]);
  }

  /**
   * Navega para a página de adicionar um evento
   */
  goToAddEvent() {
    this.router.navigate(['/add-event']);
  }
}
