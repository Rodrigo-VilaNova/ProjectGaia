import { CommonModule } from '@angular/common';
import { Component, ElementRef, Renderer2 } from '@angular/core';
import { Router, RouterModule } from '@angular/router';

@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.css',
  imports: [CommonModule, RouterModule]
})

/**
 * Componente responsável pela navbar
 */
export class NavbarComponent {

  /**
   * Construtor do componente
   * @param router Serviço de routing para navegação
   * @param el Gestor de referências para certos elementos
   * @param renderer Renderizador dos elementos da navbar
   */
  constructor(private router: Router, private el: ElementRef, private renderer: Renderer2) {
    this.router.events.subscribe(() => { this.addBoldText(); });
  }

  /**
   * Mete o texto a negrito dos botões da página atual na navbar
   */
  addBoldText() {
    const buttonIDs: {[key: string]: string} = {
      'dashboard': 'dashboard-btn',
      'invoices': 'invoices-btn',
      'events': 'events-btn',
      'simulation': 'simulation-btn',
      'about-us': 'about-us-btn',
    }

    const pageURL = this.router.url.split('/')[1]
    const buttonID: string | null = buttonIDs[pageURL] || null;
    if (buttonID !== null) {
      const element = this.el.nativeElement.querySelector(`#${buttonID}`);
      this.renderer.addClass(element, 'page-selected-btn');
    }
  }

  /**
   * Funções de navegação da navbar
   */

  goToDashboard() {
    this.router.navigate(['/dashboard']);
  }

  goToInvoices() {
    this.router.navigate(['/invoices']);
  }

  goToEvents() {
    this.router.navigate(['/events']);
  }

  goToSimulation() {
    this.router.navigate(['/simulation']);
  }

  goToAboutUs() {
    this.router.navigate(['/about-us']);
  }

  goToProfile() {
    this.router.navigate(['/account']);
  }
}
