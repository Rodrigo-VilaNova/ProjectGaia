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

@NgModule({
  declarations: [
    AppComponent
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
    AddInvoiceComponent
  ],
  providers: [
    provideHttpClient(withInterceptors([authInterceptor])),
  ],
  bootstrap: [AppComponent],
})
export class AppModule { }
