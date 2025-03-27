import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, AbstractControl, ValidationErrors } from '@angular/forms';
import { environment } from '../../environments/environment';
import { CommonModule } from '@angular/common';
import { NavbarComponent } from '../navbar/navbar.component';

@Component({
  selector: 'app-edit-event',
  templateUrl: './edit-event.component.html',
  styleUrls: ['./edit-event.component.css'],
  standalone: true,
  imports: [RouterModule, ReactiveFormsModule, CommonModule, NavbarComponent]
})
export class EditEventComponent implements OnInit {
  eventForm: FormGroup;
  eventId: number = 0;
  errorMessage: string = '';
  successMessage: string = '';

  today: string = '';

  constructor(private route: ActivatedRoute, private router: Router, private http: HttpClient, private fb: FormBuilder) {
    this.eventForm = this.fb.group({
      name: ['', [Validators.required, this.noWhiteSpaceValidator()]],
      description: ['', [Validators.required, this.noWhiteSpaceValidator()]],
      date: ['', Validators.required],
      type: [0, Validators.required]
    });

    const todayDate = new Date();
    this.today = todayDate.toISOString().split('T')[0];
  }

  ngOnInit() {
    this.route.params.subscribe(params => {
      this.eventId = params['id'];
      this.loadEvent();
    });
  }

  loadEvent() {
    this.http.get<any>(`${environment.apiUrl}/events/${this.eventId}`).subscribe(
      (event) => {
        if (event.date) {
          const formattedDate = new Date(event.date).toISOString().split('T')[0];
          this.eventForm.patchValue({
            name: event.name,
            description: event.description,
            date: formattedDate,
            type: event.type,
          });
        } else {
          this.errorMessage = 'Error: Missing event date.';
        }
      },
      (error) => {
        this.errorMessage = 'Error loading event data.';
      }
    );
  }

  submitEdit() {
    if (this.eventForm.invalid) {
      this.errorMessage = 'Please fill in all required fields.';
      return;
    }

    const selectedDateString: string = this.eventForm.value.date;
    const selectedDate = new Date(selectedDateString);

    const today = new Date();
    today.setHours(0, 0, 0, 0);

    if (selectedDate < today) {
      this.errorMessage = 'The event date must be today or later.';
      return;
    }

    const updateData = this.eventForm.value;

    this.http.put(`${environment.apiUrl}/events/${this.eventId}`, updateData).subscribe(
      () => {
        this.successMessage = 'Event updated successfully!';
        setTimeout(() => this.router.navigate(['/events']), 2000);
      },
      (error) => {
        this.errorMessage = 'Error updating event. Please try again.';
      }
    );
  }

  cancelEdit() {
    this.router.navigate(['/events']);
  }

  noWhiteSpaceValidator() {
    return (control: AbstractControl): ValidationErrors | null => {
      if (typeof control.value !== 'string') return null;

      const isWhitespace = control.value.trim().length === 0;
      return isWhitespace ? { whitespace: true } : null;
    };
  }
}
