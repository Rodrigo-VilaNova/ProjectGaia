import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, AbstractControl, ValidationErrors } from '@angular/forms';
import { environment } from '../../environments/environment';
import { CommonModule } from '@angular/common';
import { NavbarComponent } from '../navbar/navbar.component';
import { FooterComponent } from '../footer/footer.component';

@Component({
  selector: 'app-edit-event',
  templateUrl: './edit-event.component.html',
  styleUrls: ['./edit-event.component.css'],
  standalone: true,
  imports: [RouterModule, ReactiveFormsModule, CommonModule, NavbarComponent, FooterComponent]
})

/**
 * Componente responsável pela edição de um evento
 */
export class EditEventComponent implements OnInit {

  /** Formulário de edição do evento */
  eventForm: FormGroup;

  /** ID do evento a alterar */
  eventId: number = 0;

  /** Mensagem de erro exibida ao utilizador */
  errorMessage: string = '';

  /** Mensagem de sucesso exibida ao utilizador */
  successMessage: string = '';

  /** Data atual formatada como 'YYYY-MM-DD' */
  today: string = '';

  /**
   * Construtor do componente.
   * @param route Serviço para acessar parâmetros da rota ativa
   * @param router Serviço de routing para navegação
   * @param http Cliente HTTP para comunicação com a API
   * @param fb FormBuilder para criação e validação do formulário
   */
  constructor(private route: ActivatedRoute, private router: Router, private http: HttpClient, private fb: FormBuilder) {
    // Define a data de hoje e formata para validação no formulário
    const todayDate = new Date();
    this.today = todayDate.toISOString().split('T')[0];

    // Criação do formulário com validações
    this.eventForm = this.fb.group({
      name: ['', [Validators.required, this.noWhiteSpaceValidator()]],
      description: ['', [Validators.required, this.noWhiteSpaceValidator()]],
      date: ['', Validators.required],
      type: [0, Validators.required]
    });
  }

  /**
   * Método do ciclo de vida do Angular chamado quando o componente é inicializado.
   * Obtém o ID do evento da rota e carrega os seus dados.
   */
  ngOnInit() {
    this.route.params.subscribe(params => {
      this.eventId = params['id'];
      this.loadEvent();
    });
  }

  /**
   * Obtém os detalhes do evento a partir da API e preenche o formulário.
   */
  loadEvent() {
    this.http.get<any>(`${environment.apiUrl}/events/${this.eventId}`).subscribe(
      (event) => {
        if (event.date) {
          const formattedDate = new Date(event.date).toISOString().split('T')[0];
          this.eventForm.patchValue({
            name: event.name,
            description: event.description,
            date: formattedDate,
            type: event.type,
          });
        } else {
          this.errorMessage = 'Error: Missing event date.';
        }
      },
      (error) => {
        this.errorMessage = 'Error loading event data.';
      }
    );
  }

  /**
   * Submete as alterações feitas ao evento, enviando-as para a API.
   * Antes do envio, verifica se o formulário é válido e se a data selecionada não é anterior ao dia atual.
   */
  submitEdit() {
    if (this.eventForm.invalid) {
      this.errorMessage = 'Please fill in all required fields.';
      return;
    }

    const selectedDateString: string = this.eventForm.value.date;
    const selectedDate = new Date(selectedDateString);

    const today = new Date();
    today.setHours(0, 0, 0, 0);

    if (selectedDate < today) {
      this.errorMessage = 'The event date must be today or later.';
      return;
    }

    const updateData = this.eventForm.value;

    this.http.put(`${environment.apiUrl}/events/${this.eventId}`, updateData).subscribe(
      () => {
        this.successMessage = 'Event updated successfully!';
        this.router.navigate(['/events']);
      },
      (error) => {
        this.errorMessage = 'Error updating event. Please try again.';
      }
    );
  }

  /**
   * Cancela a edição e redireciona o utilizador para a lista de eventos.
   */
  cancelEdit() {
    this.router.navigate(['/events']);
  }

  /**
   * Validação personalizada para impedir que o usuário insira apenas espaços em branco
   * @returns Um erro de validação se o campo contiver apenas espaços em branco
   */
  noWhiteSpaceValidator() {
    return (control: AbstractControl): ValidationErrors | null => {
      if (typeof control.value !== 'string') return null;

      const isWhitespace = control.value.trim().length === 0;
      return isWhitespace ? { whitespace: true } : null;
    };
  }
}
