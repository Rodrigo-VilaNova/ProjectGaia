import { Component } from '@angular/core';
import { Router, RouterModule } from '@angular/router';

@Component({
  selector: 'app-register-confirm',
  standalone: true,
  templateUrl: './register-confirm.component.html',
  styleUrl: './register-confirm.component.css',
  imports: [RouterModule]
})
export class RegisterConfirmComponent {
  constructor(private router: Router) { }

  goToLandingPage() {
    this.router.navigate(['']);
  }
}
