import { Component } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { FooterComponent } from '../footer/footer.component';
import { NavbarSimpleComponent } from '../navbar-simple/navbar-simple.component';

@Component({
  selector: 'app-register-confirm',
  standalone: true,
  templateUrl: './register-confirm.component.html',
  styleUrl: './register-confirm.component.css',
  imports: [RouterModule, NavbarSimpleComponent, FooterComponent]
})

/**
 * Componente responsável pela página de sucesso no registo
 * Serve apenas para informar ao utilizador que deve consultar o email para aceder à aplicação
 */
export class RegisterConfirmComponent {
  constructor(private router: Router) { }

  goToLandingPage() {
    this.router.navigate(['']);
  }
}
