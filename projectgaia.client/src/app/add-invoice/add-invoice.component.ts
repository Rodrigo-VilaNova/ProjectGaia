import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-add-invoice',
  templateUrl: './add-invoice.component.html',
  styleUrls: ['./add-invoice.component.css'],
  standalone: true,
  imports: [RouterModule, CommonModule, ReactiveFormsModule],
})
export class AddInvoiceComponent {
  invoiceForm: FormGroup;

  invoice = {
    price: null,
    consumption: null,
    emissionDate: null,
  };

  errorMessage: string = '';
  successMessage: string = '';

  constructor(private http: HttpClient, private router: Router, private fb: FormBuilder) {
    this.invoiceForm = this.fb.group({
      price: [''],
      consumption: [''],
      emissionDate: [''],
    });
  }

  submitInvoice() {
    /*this.http.post('https://localhost:7277/api/invoices', this.invoice).subscribe(
      (response) => {
        this.successMessage = 'Invoice added successfully!';
        setTimeout(() => {
          this.router.navigate(['/invoices']); // Redireciona para a página das faturas
        }, 2000);
      },
      (error) => {
        this.errorMessage = 'Error adding invoice. Please try again.';
      }
    );*/
    console.log(this.invoiceForm.value);
  }

  goToDashboard() {
    this.router.navigate(['/dashboard']);
  }

  goToInvoices() {
    this.router.navigate(['/invoices']);
  }
}

