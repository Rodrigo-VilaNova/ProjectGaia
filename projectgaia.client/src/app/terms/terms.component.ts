import { Component } from '@angular/core';
import { NavbarSimpleComponent } from '../navbar-simple/navbar-simple.component';
import { FooterComponent } from '../footer/footer.component';

@Component({
  selector: 'app-terms',
  templateUrl: './terms.component.html',
  styleUrl: './terms.component.css',
  imports: [NavbarSimpleComponent, FooterComponent]
})

/**
 * Componente responsável pela página de termos e condições
 */
export class TermsComponent {

}
