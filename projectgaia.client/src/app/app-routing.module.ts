import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { LandingPageComponent } from './landing-page/landing-page.component';
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './register/register.component';
import { RegisterConfirmComponent } from './register-confirm/register-confirm.component';
import { RegisteredComponent } from './registered/registered.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { InvoicesComponent } from './invoices/invoices.component';
import { AddInvoiceComponent } from './add-invoice/add-invoice.component';
import { AuthGuard } from './auth.guard';


const routes: Routes = [
  { path: '', component: LandingPageComponent, pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'register-sent', component: RegisterConfirmComponent },
  { path: 'confirm', component: RegisteredComponent },
  { path: 'dashboard', component: DashboardComponent, canActivate: [AuthGuard] },
  { path: 'invoices', component: InvoicesComponent, canActivate: [AuthGuard] },
  { path: 'add-invoice', component: AddInvoiceComponent, canActivate: [AuthGuard] },
  { path: '**', redirectTo: '' },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
