import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { LandingPageComponent } from './landing-page/landing-page.component';
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './register/register.component';
import { RegisterConfirmComponent } from './register-confirm/register-confirm.component';
import { RegisteredComponent } from './registered/registered.component';
import { InvoicesComponent } from './invoices/invoices.component';
import { authInterceptor } from './interceptors/auth.interceptor';
import { AddInvoiceComponent } from './add-invoice/add-invoice.component';
import { RecoveryComponent } from './recovery/recovery.component';
import { ResetPasswordComponent } from './reset-password/reset-password.component';
import { ChangePasswordComponent } from './change-password/change-password.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { ProfileComponent } from './profile/profile.component';
import { EventsComponent } from './events/events.component';
import { AddEventComponent } from './add-event/add-event.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { NavbarComponent } from './navbar/navbar.component';
import { EditEventComponent } from './edit-event/edit-event.component';
import { EditInvoiceComponent } from './edit-invoice/edit-invoice.component';
import { SimulationComponent } from './simulation/simulation.component';
import { AboutUsComponent } from './about-us/about-us.component';

/**
 * Declaração de todos os componentes utilizados
 * Todos os componentes estão no imports por serem standalone
 */
@NgModule({
  declarations: [
    AppComponent,
    AboutUsComponent,
  ], // Only AppComponent should be declared here
  imports: [
    BrowserModule,
    AppRoutingModule,
    FormsModule,
    ReactiveFormsModule,
    LandingPageComponent,
    LoginComponent,
    RegisterComponent,
    RegisterConfirmComponent,
    RegisteredComponent,
    RecoveryComponent,
    ResetPasswordComponent,
    ChangePasswordComponent,
    DashboardComponent,
    InvoicesComponent,
    ProfileComponent,
    AddInvoiceComponent,
    EventsComponent,
    AddEventComponent,
    NavbarComponent,
    EditEventComponent,
    EditInvoiceComponent,
    BrowserAnimationsModule,
    SimulationComponent,
  ],
  providers: [
    provideHttpClient(withInterceptors([authInterceptor])),
  ],
  bootstrap: [AppComponent],
})
export class AppModule { }
