import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-navbar-simple',
  imports: [],
  templateUrl: './navbar-simple.component.html',
  styleUrl: './navbar-simple.component.css'
})
export class NavbarSimpleComponent {
  constructor(private router: Router) {
  }

  goToDashboard() {
    this.router.navigate(['']);
  }
}
