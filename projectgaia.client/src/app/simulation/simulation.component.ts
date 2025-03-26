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
export class SimulationComponent {

  simulationForm: FormGroup;
  providerName: string = '';
  providerPrice: number | null = null;
  recommendation: string = '';

  invoices: Invoice[] = [];

  averagePrice: number = 0;
  averageConsumption: number = 0;
  averagePricePerKWH: number = 0;

  constructor(private http: HttpClient, private invoiceService: InvoiceService, private fb: FormBuilder) {
    this.simulationForm = this.fb.group({
      providerName: ['', Validators.required],
      providerPrice: ['', [Validators.required, Validators.min(0.01)]]
    });
    this.loadAverageData();
  }

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
      } else {
        this.recommendation = `Staying with your current provider is more cost-effective. (Current: €${this.averagePricePerKWH}/kWh, New: €${providerPrice}/kWh)`;
      }
    } else {
      this.recommendation = 'Could not retrieve current provider data.';
    }
  }
}
