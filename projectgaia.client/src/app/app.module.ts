import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { LandingPageComponent } from './landing-page/landing-page.component';
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './register/register.component';
import { InvoicesComponent } from './invoices/invoices.component';
import { ReactiveFormsModule } from '@angular/forms';
import { authInterceptor } from './interceptors/auth.interceptor';
import { RegisterConfirmComponent } from './register-confirm/register-confirm.component';

@NgModule({
  declarations: [
    AppComponent,
  ], // Only AppComponent should be declared here
  imports: [
    BrowserModule,
    AppRoutingModule,
    LandingPageComponent,
    LoginComponent,
    RegisterComponent,
    InvoicesComponent,
    ReactiveFormsModule,
    RegisterConfirmComponent,
  ],
  providers: [
    provideHttpClient(withInterceptors([authInterceptor])),
  ],
  bootstrap: [AppComponent],
})
export class AppModule { }
