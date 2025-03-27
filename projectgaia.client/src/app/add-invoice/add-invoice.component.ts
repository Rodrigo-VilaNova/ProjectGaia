import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, AbstractControl, ValidationErrors } from '@angular/forms';
import { environment } from '../../environments/environment';
import { NavbarComponent } from '../navbar/navbar.component';

@Component({
  selector: 'app-add-invoice',
  templateUrl: './add-invoice.component.html',
  styleUrls: ['./add-invoice.component.css'],
  standalone: true,
  imports: [RouterModule, CommonModule, ReactiveFormsModule, NavbarComponent],
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

  today: string = '';

  constructor(private http: HttpClient, private router: Router, private fb: FormBuilder) {
    const todayDate = new Date();
    this.today = todayDate.toISOString().split('T')[0];

    this.invoiceForm = this.fb.group({
      price: ['', [Validators.required, this.noWhiteSpaceValidator()]],
      consumption: ['', [Validators.required, this.noWhiteSpaceValidator()]],
      emissionDate: ['', Validators.required],
    });
  }

  isSubmitting = false;

  submitInvoice() {
    if (this.invoiceForm.invalid) {
      this.errorMessage = 'Please fill in all fields before submitting.';
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

  noWhiteSpaceValidator() {
    return (control: AbstractControl): ValidationErrors | null => {
      if (typeof control.value !== 'string') return null;

      const isWhitespace = control.value.trim().length === 0;
      return isWhitespace ? { whitespace: true } : null;
    };
  }

  goToInvoices() {
    this.router.navigate(['/invoices']);
  }
}

