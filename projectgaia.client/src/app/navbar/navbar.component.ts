import { CommonModule } from '@angular/common';
import { Component, ElementRef, Renderer2 } from '@angular/core';
import { Router, RouterModule } from '@angular/router';

@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.css',
  imports: [CommonModule, RouterModule]
})
export class NavbarComponent {
  constructor(private router: Router, private el: ElementRef, private renderer: Renderer2) {
    this.router.events.subscribe(() => { this.addBoldText(); });
  }

  addBoldText() {
    const buttonIDs: {[key: string]: string} = {
      'dashboard': 'dashboard-btn',
      'invoices': 'invoices-btn',
      'events': 'events-btn',
      'simulation': 'simulation-btn',
    }

    const pageURL = this.router.url.split('/')[1]
    const buttonID: string | null = buttonIDs[pageURL] || null;
    if (buttonID !== null) {
      const element = this.el.nativeElement.querySelector(`#${buttonID}`);
      this.renderer.addClass(element, 'page-selected-btn');
    }
  }

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

  goToProfile() {
    this.router.navigate(['/account']);
  }
}
