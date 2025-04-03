import { Component } from '@angular/core';
import { Router, RouterModule } from '@angular/router';

@Component({
  selector: 'app-landing-page',
  templateUrl: './landing-page.component.html',
  styleUrls: ['./landing-page.component.css'],
  standalone: true,
  imports: [RouterModule]
})

/**
 * Componente da Landing Page
 * Contém apenas métodos de navegação
 */
export class LandingPageComponent {
  constructor(private router: Router) {}

  navigateToLogin() {
    console.log("Navigating to login...")
    this.router.navigate(['login']);
  }

  navigateToRegister() {
    console.log("Navigating to register...")
    this.router.navigate(['register']);
  }

  navigateToForgotPassword() {
    console.log("Navigating to forgot password...")
    this.router.navigate(['recovery']);
  }
}
