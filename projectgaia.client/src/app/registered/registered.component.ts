import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { CommonModule } from '@angular/common';


@Component({
  selector: 'app-registered',
  templateUrl: './registered.component.html',
  styleUrls: ['./registered.component.css'],
  imports: [CommonModule]
})

/**
 * Componente responsável por tratar do processo de registo
 */
export class RegisteredComponent implements OnInit {

  /** Mensagem a ser exibida */
  message: string = '';

  /** Averigua se o processo de registo está a ser processado */
  isLoading: boolean = true;

  /**
   * Contrutor do componente
   * @param router Serviço de routing para navegação
   * @param http Cliente HTTP para comunicação com a API
   * @param activatedRoute A rota onde o token de autenticação vai ser buscado
   */
  constructor(private router: Router, private http: HttpClient, private activatedRoute: ActivatedRoute) { }

  /**
   * Método do ciclo de vida do Angular chamado quando o componente é inicializado.
   * Inicia o processo de registo
   */
  ngOnInit(): void {
    this.registerAccount();
  }

  /**
   * Responsável pelo processo de registo
   * Usa a activatedRoute para extraír o token de autenticação e dar acesso da aplicação ao utilizador
   */
  registerAccount(): void {
    const token = this.activatedRoute.snapshot.queryParamMap.get('token');

    this.http.get(`${environment.apiUrl}/account/confirm?token=${token}`, { responseType: 'text' })
      .subscribe({
        next: (response: string) => {
          this.message = response ;
          this.isLoading = false;
        },
        error: (error) => {
          this.message = error.error || `An unexpected error occured with no message, error code ${error.status}`;
          this.isLoading = false;
        }
      });
  }

  /**
   * Navega para a landing page
   */
  returnToLanding(): void {
    this.router.navigate(['/landing']);
  }
}
