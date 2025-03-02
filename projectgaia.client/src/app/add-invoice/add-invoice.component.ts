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

  isSubmitting = false;

  submitInvoice() {
    // Verifica se todos os campos estão preenchidos
    if (!this.invoice.price || !this.invoice.consumption || !this.invoice.emissionDate) {
      this.errorMessage = 'Please fill in all fields before submitting.';
      return;
    }

    if (this.isSubmitting) return;
    this.isSubmitting = true;

    const invoiceDTO = {
      price: this.invoice.price,
      consumption: this.invoice.consumption,
      emissionDate: this.invoice.emissionDate
    };

    this.http.post('https://localhost:7277/api/invoices', invoiceDTO).subscribe(
      (response) => {
        this.successMessage = 'Invoice added successfully!';

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

  goToDashboard() {
    this.router.navigate(['/dashboard']);
  }

  goToInvoices() {
    this.router.navigate(['/invoices']);
  }
}

