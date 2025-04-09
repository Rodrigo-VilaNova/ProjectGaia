import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-navbar-simple',
  imports: [],
  templateUrl: './navbar-simple.component.html',
  styleUrl: './navbar-simple.component.css'
})

/**
 * Componente responsável pela navbar simplificada
 */
export class NavbarSimpleComponent {
  constructor(private router: Router) {
  }

  /**
   * Funções de navegação da navbar simplificada
   */

  goToDashboard() {
    this.router.navigate(['']);
  }
}
