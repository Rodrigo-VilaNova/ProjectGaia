import { Component } from '@angular/core';
import { NavbarSimpleComponent } from '../navbar-simple/navbar-simple.component';
import { FooterComponent } from '../footer/footer.component';

@Component({
  selector: 'app-about-us',
  templateUrl: './about-us.component.html',
  styleUrl: './about-us.component.css',
  standalone: true,
  imports: [NavbarSimpleComponent, FooterComponent]
})

/**
 * Componente responsável pela página de about us
 */
export class AboutUsComponent {

}
