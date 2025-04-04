import { Component, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { InvoiceService, Invoice } from '../services/invoice.service';
import { environment } from '../../environments/environment';
import { FormsModule } from '@angular/forms';
import { NavbarComponent } from '../navbar/navbar.component';

@Component({
  selector: 'app-invoices',
  templateUrl: './invoices.component.html',
  styleUrls: ['./invoices.component.css'],
  standalone: true,
  imports: [RouterModule, CommonModule, FormsModule, NavbarComponent]
})

/**
 * Componente responsável pelas faturas
 */
export class InvoicesComponent implements OnInit {

  /** Lista de faturas */
  invoices: Invoice[] = [];

  /** Lista das faturas filtradas*/
  filteredInvoices: Invoice[] = [];

  /** Lista das faturas selecionadas*/
  selectedInvoices: number[] = [];

  showFilterOverlay = false;

  /** Coluna a ser filtrada */
  currentSortColumn: string | null = null;

  /** Ordem do filtro */
  currentSortOrder: 'asc' | 'desc' | null = null;

  /** Dicionário dos filtros disponíveis */
  static readonly FILTERS_DEFAULT = {
    id: { min: null as number | null, max: null as number | null },
    price: { min: null as number | null, max: null as number | null },
    consumption: { min: null as number | null, max: null as number | null },
    emissionDate: { min: null as Date | null, max: null as Date | null },
    uploadDate: { min: null as Date | null, max: null as Date | null }
  };

  /** Seleciona quais os filtros que serão possíveis de ser aplicados */
  filters = structuredClone(InvoicesComponent.FILTERS_DEFAULT);

  hasFiltersApplied = this.isFilterModified();

  /**
   * Construtor do componente
   * @param router Serviço de routing para navegação
   * @param http Cliente HTTP para comunicação com a API
   * @param invoiceService~Serviço responsável pelas faturas
   */
  constructor(private router: Router, private http: HttpClient, private invoiceService: InvoiceService) { }

  /**
   * Método do ciclo de vida do Angular chamado quando o componente é inicializado.
   * Carrega as faturas na base de dados.
   */
  ngOnInit() {
    this.loadInvoices();
  }

  /**
   * Mostra ou esconde o overlay dos filtros
   */
  toggleFilterOverlay() {
    this.showFilterOverlay = !this.showFilterOverlay;
  }

  /**
   * Busca todos as faturas associadas ao ID do utilizador respetivo
   */
  loadInvoices() {
    this.invoiceService.getUserInvoices().subscribe(
      (data) => {
        this.invoices = data;
        this.applyFilters();
      },
      (error) => {
        console.error('Error fetching invoices:', error);
      }
    );
  }

  /**
   * Averigua se um filtro foi modificado
   * @returns True caso algum filtro tenha sido alterado, False caso contrário
   */
  isFilterModified(): boolean {
  for (let key in this.filters) {
    const filter = this.filters[key as keyof typeof this.filters];
    const default_filter = InvoicesComponent.FILTERS_DEFAULT[key as keyof typeof InvoicesComponent.FILTERS_DEFAULT];

    if (filter.min !== default_filter.min ||
      filter.max !== default_filter.max) {
      return true;
    }
  }

    return false;
  }

  /**
   * Aplica os filtros escolhidos
   */
  applyFilters() {
    this.hasFiltersApplied = this.isFilterModified();

    this.filteredInvoices = [...this.invoices];

    this.filteredInvoices = this.filteredInvoices.filter(invoice =>
      (this.filters.id.min == null || invoice.id >= this.filters.id.min) &&
      (this.filters.id.max == null || invoice.id <= this.filters.id.max) &&
      (this.filters.price.min == null || invoice.price >= this.filters.price.min) &&
      (this.filters.price.max == null || invoice.price <= this.filters.price.max) &&
      (this.filters.consumption.min == null || invoice.consumption >= this.filters.consumption.min) &&
      (this.filters.consumption.max == null || invoice.consumption <= this.filters.consumption.max) &&
      (this.filters.emissionDate.min == null || invoice.emissionDate >= this.filters.emissionDate.min) &&
      (this.filters.emissionDate.max == null || invoice.emissionDate <= this.filters.emissionDate.max) &&
      (this.filters.uploadDate.min == null || invoice.emissionDate >= this.filters.uploadDate.min) &&
      (this.filters.uploadDate.max == null || invoice.emissionDate <= this.filters.uploadDate.max)
    );
  }

  /**
   * Reset nos filtros selecionados
   */
  resetFilters() {
    this.filters = structuredClone(InvoicesComponent.FILTERS_DEFAULT);
    this.applyFilters();
  }

  /**
   * Permite a seleção de uma fatura
   * @param invoiceId O ID da fatura selecionada
   * @param event O evento de dar check á checkbox
   */
  toggleInvoiceSelection(invoiceId: number, event: globalThis.Event) {
    const isChecked = (event.target as HTMLInputElement).checked;
    if (isChecked) {
      this.selectedInvoices.push(invoiceId);
    } else {
      this.selectedInvoices = this.selectedInvoices.filter(id => id !== invoiceId);
    }
    this.applyFilters(); // Update table to show only selected invoices
  }

  /**
   * Apaga as faturas selecionadas
   * @returns A lista de faturas sem as faturas selecionadas anteriormente
   */
  deleteSelectedInvoices() {
    if (this.selectedInvoices.length === 0) return;
    if (!confirm("Are you sure you wish to delete selected invoices?")) return;

    const deleteRequests = this.selectedInvoices.map(id =>
      this.http.delete(`${environment.apiUrl}/invoices/${id}`).toPromise()
    );

    Promise.all(deleteRequests)
      .then(() => {
        this.invoices = this.invoices.filter(invoice => !this.selectedInvoices.includes(invoice.id));
        this.selectedInvoices = [];
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
  sortBy(column: keyof Invoice) {
    if (this.currentSortColumn === column) {
      this.currentSortOrder = this.currentSortOrder === 'asc' ? 'desc' : 'asc';
    } else {
      this.currentSortColumn = column;
      this.currentSortOrder = 'asc';
    }

    this.filteredInvoices.sort((a, b) => {
      if (a[column] < b[column]) return this.currentSortOrder === 'asc' ? -1 : 1;
      if (a[column] > b[column]) return this.currentSortOrder === 'asc' ? 1 : -1;
      return 0;
    });
  }

  /**
   * Navega para a página de edição caso apenas 1 fatura seja selecionada
   * @returns Uma mensagem de erro caso mais de 1 fatura seja selecionada
   */
  editSelectedInvoice() {
    if (this.selectedInvoices.length !== 1) {
      alert("Please select exactly one invoice to edit.");
      return;
    }

    const invoiceId = this.selectedInvoices[0];
    this.router.navigate([`/edit-invoice/${invoiceId}`]);
  }

  /**
   * Navega para a página de adicionar uma fatura
   */
  goToAddInvoice() {
    this.router.navigate(['/add-invoice']);
  }
}
