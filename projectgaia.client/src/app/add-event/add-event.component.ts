import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { environment } from '../../environments/environment';

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
  imports: [RouterModule, CommonModule, ReactiveFormsModule],
})
export class AddEventComponent {

  eventForm: FormGroup;
  errorMessage: string = '';
  successMessage: string = '';
  today: string = '';

  constructor(private http: HttpClient, private router: Router, private fb: FormBuilder) {
    const todayDate = new Date();
    this.today = todayDate.toISOString().split('T')[0]; 

    this.eventForm = this.fb.group({
      name: ['', Validators.required],
      description: ['', Validators.required],
      date: ['', Validators.required],
      type: [0, Validators.required],
    });
  }

  isSubmitting = false;

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

  goToDashboard() {
    this.router.navigate(['/dashboard']);
  }

  goToEvents() {
    this.router.navigate(['/events']);
  }
}
