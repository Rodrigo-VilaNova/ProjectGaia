import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, AbstractControl, ValidationErrors } from '@angular/forms';
import { environment } from '../../environments/environment';
import { NavbarComponent } from '../navbar/navbar.component';
import { FooterComponent } from '../footer/footer.component';

@Component({
  selector: 'app-add-invoice',
  templateUrl: './add-invoice.component.html',
  styleUrls: ['./add-invoice.component.css'],
  standalone: true,
  imports: [RouterModule, CommonModule, ReactiveFormsModule, NavbarComponent, FooterComponent],
})

/**
  * Componente responsável pela criação de novas faturas.
  */
export class AddInvoiceComponent {
  /** Formulário para adicionar uma nova fatura */
  invoiceForm: FormGroup;

  /** Representação dos dados de uma fatura */
  invoice = {
    price: null,
    consumption: null,
    emissionDate: null,
  };

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
    this.invoiceForm = this.fb.group({
      price: ['', [Validators.required, this.noWhiteSpaceValidator()]],
      consumption: ['', [Validators.required, this.noWhiteSpaceValidator()]],
      emissionDate: ['', Validators.required],
    });
  }

  /**
   * Submete a fatura para a API após validações
   * Antes do envio, verifica se o formulário é válido e se a data selecionada não é superior ao dia atual.
   */
  submitInvoice() {
    if (this.invoiceForm.invalid) {
      this.errorMessage = 'Please fill in all fields before submitting.';
      return;
    }

    const selectedDateString: string = this.invoiceForm.value.emissionDate;
    const selectedDate = new Date(selectedDateString);
    selectedDate.setHours(0, 0, 0, 0);

    const today = new Date();
    today.setHours(0, 0, 0, 0);

    if (selectedDate > today) {
      this.errorMessage = 'The event date must be today or sooner.';
      return;
    }

    if (this.isSubmitting) return;
    this.isSubmitting = true;

    const invoiceDTO = {
      price: this.invoiceForm.value.price,
      consumption: this.invoiceForm.value.consumption,
      emissionDate: this.invoiceForm.value.emissionDate,
    };

    if (this.invoiceForm.value.emissionDate > today) {
      this.errorMessage = 'The event date must be today or sooner.';
      this.isSubmitting = false;
      return;
    }

    this.http.post(`${environment.apiUrl}/invoices`, invoiceDTO).subscribe(
      (response) => {
        this.successMessage = 'Invoice added successfully!';
        this.errorMessage = '';
        this.router.navigate(['/invoices']);
      },
      (error) => {
        console.error('Error adding invoice:', error);
        this.errorMessage = error.error || 'Error adding invoice. Please try again.';
      }
    ).add(() => {
      this.isSubmitting = false;
    });
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

  /**
   * Redireciona o utilizador para a página de faturas
   */
  goToInvoices() {
    this.router.navigate(['/invoices']);
  }
}

