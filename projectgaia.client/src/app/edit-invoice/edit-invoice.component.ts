import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, AbstractControl, ValidationErrors } from '@angular/forms';
import { environment } from '../../environments/environment';
import { CommonModule } from '@angular/common';
import { NavbarComponent } from '../navbar/navbar.component';
import { FooterComponent } from '../footer/footer.component';

@Component({
  selector: 'app-edit-invoice',
  templateUrl: './edit-invoice.component.html',
  styleUrl: './edit-invoice.component.css',
  standalone: true,
  imports: [RouterModule, ReactiveFormsModule, CommonModule, NavbarComponent, FooterComponent]
})

/**
  * Componente responsável pela edição de uma fatura
  */
export class EditInvoiceComponent implements OnInit {

  /** Formulário para edição de uma fatura */
  invoiceForm: FormGroup;

  /** ID da fatura a alterar */
  invoiceId: number = 0;

  /** Mensagem de erro exibida ao utilizador */
  errorMessage: string = '';

  /** Mensagem de sucesso exibida ao utilizador */
  successMessage: string = '';

  /** Data atual formatada como 'YYYY-MM-DD' */
  today: string = '';

  /**
   * Construtor do componente
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
    this.invoiceForm = this.fb.group({
      price: ['', [Validators.required, this.noWhiteSpaceValidator()]],
      consumption: ['', [Validators.required, this.noWhiteSpaceValidator()]],
      emissionDate: ['', Validators.required],
    });
  }

  /**
   * Método do ciclo de vida do Angular chamado quando o componente é inicializado.
   * Obtém o ID da fatura da rota e carrega os seus dados.
   */
  ngOnInit() {
    this.route.params.subscribe(params => {
      this.invoiceId = params['id'];
      this.loadInvoice();
    });
  }

  /**
   * Obtém os detalhes da fatura a partir da API e preenche o formulário.
   */
  loadInvoice() {
    this.http.get<any>(`${environment.apiUrl}/invoices/${this.invoiceId}`).subscribe(
      (invoice) => {
        if (invoice.emissionDate) {
          const formattedDate = new Date(invoice.emissionDate).toISOString().split('T')[0];
          this.invoiceForm.patchValue({
            price: invoice.price,
            consumption: invoice.consumption,
            emissionDate: formattedDate
          });
        } else {
          this.errorMessage = 'Error: Missing emission date.';
        }
      },
      (error) => {
        this.errorMessage = 'Error loading invoice data.';
      }
    );
  }

  /**
   * Submete as alterações feitas à fatura, enviando-as para a API.
   * Antes do envio, verifica se o formulário é válido e se a data de emissão selecionada não é superior ao dia atual.
   */
  submitEdit() {
    if (this.invoiceForm.invalid) {
      this.errorMessage = 'Please fill in all required fields.';
      return;
    }

    const selectedDateString: string = this.invoiceForm.value.emissionDate;
    const selectedDate = new Date(selectedDateString);

    const today = new Date();
    today.setHours(0, 0, 0, 0);

    if (selectedDate > today) {
      this.errorMessage = 'The event date must be today or sooner.';
      return;
    }

    const updateData = this.invoiceForm.value;

    this.http.put(`${environment.apiUrl}/invoices/${this.invoiceId}`, updateData).subscribe(
      () => {
        this.successMessage = 'Invoice updated successfully!';
        setTimeout(() => this.router.navigate(['/invoices']), 2000);
      },
      (error) => {
        this.errorMessage = 'Error updating invoice. Please try again.';
      }
    );
  }

  /**
   * Cancela a edição e redireciona o utilizador para a lista de faturas.
   */
  cancelEdit() {
    this.router.navigate(['/invoices']);
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
