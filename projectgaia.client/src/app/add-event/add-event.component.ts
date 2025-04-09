import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, AbstractControl, ValidationErrors } from '@angular/forms';
import { environment } from '../../environments/environment';
import { NavbarComponent } from '../navbar/navbar.component';
import { FooterComponent } from '../footer/footer.component';

/**
 * Interface que representa os dados necessários para criar um evento.
 */
export interface EventDTO {
  name: string;
  description: string;
  date: string;
  type: number;
}

@Component({
  selector: 'app-add-event',
  templateUrl: './add-event.component.html',
  styleUrl: './add-event.component.css',
  standalone: true,
  imports: [RouterModule, CommonModule, ReactiveFormsModule, NavbarComponent, FooterComponent]
})

/**
 * Componente responsável pela criação de novos eventos.
 */
export class AddEventComponent {

  /** Formulário para adicionar um evento */
  eventForm: FormGroup;

  /** Mensagem de erro exibida ao utilizador */
  errorMessage: string = '';

  /** Mensagem de sucesso exibida ao utilizador */
  successMessage: string = '';

  /** Data atual formatada como 'YYYY-MM-DD' */
  today: string = '';

  /** Indica se o formulário está em processo de submissão para evitar múltiplos envios */
  isSubmitting = false;

  /**
   * Construtor do componente
   * @param http Serviço HTTP para comunicação com a API
   * @param router Serviço de routing para navegação
   * @param fb FormBuilder para criação e validação do formulário
   */
  constructor(private http: HttpClient, private router: Router, private fb: FormBuilder) {
    // Define a data de hoje e formata para validação no formulário
    const todayDate = new Date();
    this.today = todayDate.toISOString().split('T')[0]; 

    // Criação do formulário com validações
    this.eventForm = this.fb.group({
      name: ['', [Validators.required, this.noWhiteSpaceValidator()]],
      description: ['', [Validators.required, this.noWhiteSpaceValidator()]],
      date: ['', Validators.required],
      type: [0, Validators.required],
    });
  }

  /**
   * Submete o evento para a API após validações
   * Antes do envio, verifica se o formulário é válido e se a data selecionada não é anterior ao dia atual.
   */
  submitEvent() {
    if (this.eventForm.invalid) {
      this.errorMessage = 'Please fill in all fields before submitting.';
      return;
    }

    const selectedDate = new Date(this.eventForm.value.date);
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    if (selectedDate < today) {
      this.errorMessage = 'The event date must be today or later.';
      return;
    }

    if (this.isSubmitting) return;
    this.isSubmitting = true;

    const eventDTO = {
      name: this.eventForm.value.name,
      description: this.eventForm.value.description,
      date: this.eventForm.value.date,
      type: Number(this.eventForm.value.type)
    };

    this.http.post(`${environment.apiUrl}/events`, eventDTO).subscribe(
      (response) => {
        this.successMessage = 'Event added successfully!';
        this.errorMessage = '';
        this.router.navigate(['/events']);
      },
      (error) => {
        console.error('Error adding event:', error);
        this.errorMessage = error.error?.message || error.error || 'Error adding event. Please try again.';
      }
    ).add(() => {
      this.isSubmitting = false;
    });
  }

  /**
   * Redireciona o utilizador para a página de eventos
   */
  goToEvents() {
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
