import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { environment } from '../../environments/environment';
import { CommonModule } from '@angular/common';
import { NavbarComponent } from '../navbar/navbar.component';

@Component({
  selector: 'app-edit-invoice',
  templateUrl: './edit-invoice.component.html',
  styleUrl: './edit-invoice.component.css',
  standalone: true,
  imports: [RouterModule, ReactiveFormsModule, CommonModule, NavbarComponent]
})
export class EditInvoiceComponent implements OnInit {
  invoiceForm: FormGroup;
  invoiceId: number = 0;
  errorMessage: string = '';
  successMessage: string = '';

  today: string = '';

  constructor(private route: ActivatedRoute, private router: Router, private http: HttpClient, private fb: FormBuilder) {
    this.invoiceForm = this.fb.group({
      price: ['', Validators.required],
      consumption: ['', Validators.required],
      emissionDate: ['', Validators.required]
    });

    const todayDate = new Date();
    this.today = todayDate.toISOString().split('T')[0];
  }

  ngOnInit() {
    this.route.params.subscribe(params => {
      this.invoiceId = params['id'];
      this.loadInvoice();
    });
  }

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

  cancelEdit() {
    this.router.navigate(['/invoices']);
  }
}
