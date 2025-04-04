import { Component } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule, FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { NavbarComponent } from '../navbar/navbar.component';
import { Invoice, InvoiceService } from '../services/invoice.service';

@Component({
  selector: 'app-simulation',
  templateUrl: './simulation.component.html',
  styleUrls: ['./simulation.component.css'],
  standalone: true,
  imports: [RouterModule, CommonModule, ReactiveFormsModule, NavbarComponent]
})

/**
 * Componente responsável pelo processo de simulação de faturas
 */
export class SimulationComponent {

  /** Formulário de simulação */
  simulationForm: FormGroup;

  /** Nome da fornecedora simulada */
  providerName: string = '';

  /** Custo por kWh da fornecedora simulada */
  providerPrice: number | null = null;

  /** A mensagem da recomendação */
  recommendation: string = '';

  /** As faturas do utilizador */
  invoices: Invoice[] = [];

  /** Custo médio das faturas */
  averagePrice: number = 0;

  /** Consumo médio das faturas */
  averageConsumption: number = 0;

  /** Custo por kWh médio das faturas */
  averagePricePerKWH: number = 0;

  /**
   * Construtor do componente
   * @param http Cliente HTTP para comunicação com a API
   * @param invoiceService Serviço responsável pelas faturas
   * @param fb FormBuilder para criação e validação do formulário
   */
  constructor(private http: HttpClient, private invoiceService: InvoiceService, private fb: FormBuilder) {
    // Criação do formulário com validações
    this.simulationForm = this.fb.group({
      providerName: ['', Validators.required],
      providerPrice: ['', [Validators.required, Validators.min(0.01)]]
    });
    this.loadAverageData();
  }

  /**
   * Carrega as informações necessárias para os cálculos na simulação
   */
  loadAverageData() {
    this.invoiceService.getUserInvoices().subscribe(
      (data) => {
        this.invoices = data;
        this.averagePrice = parseFloat((this.invoices.reduce((sum, invoice) => sum + invoice.price, 0) / this.invoices.length).toFixed(2));
        this.averageConsumption = parseFloat((this.invoices.reduce((sum, invoice) => sum + invoice.consumption, 0) / this.invoices.length).toFixed(2));
        this.averagePricePerKWH = parseFloat((this.averagePrice / this.averageConsumption).toFixed(2));
      },
      (error) => {
        console.error('Error fetching invoices:', error);
      }
    );
  }

  /**
   * Responsável pelo processo da simulação
   * Averigua o preço por kWh simulado e compara ao médio atual do utilizador
   */
  simulateComparison() {
    if (this.simulationForm.invalid) {
      this.recommendation = 'Please fill in all fields correctly.';
      return;
    }

    const providerName = this.simulationForm.value.providerName;
    const providerPrice = this.simulationForm.value.providerPrice;

    if (this.averagePricePerKWH !== null) {
      if (providerPrice < this.averagePricePerKWH) {
        this.recommendation = `Switching to ${providerName} could save you money! (Current: €${this.averagePricePerKWH}/kWh, New: €${providerPrice}/kWh)`;
      } else if (providerPrice > this.averagePricePerKWH) {
        this.recommendation = `Staying with your current provider is more cost-effective. (Current: €${this.averagePricePerKWH}/kWh, New: €${providerPrice}/kWh)`;
      } else {
        this.recommendation = `Both your provider and ${providerName} have the same cost per kWh. (Current: €${this.averagePricePerKWH}/kWh, New: €${providerPrice}/kWh)`;
      }
    } else {
      this.recommendation = 'Could not retrieve current provider data.';
    }
  }
}
