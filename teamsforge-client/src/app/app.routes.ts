import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { LoginComponent } from './features/auth/login/login';
import { RegisterComponent } from './features/auth/register/register';
import { ForgotpasswordComponent } from './features/auth/forgotpassword/forgotpassword';
import { ResetpasswordComponent } from './features/auth/resetpassword/resetpassword';
import { DashboardComponent } from './features/dashboard/dashboard';

export const routes: Routes = [
	{ path: '', pathMatch: 'full', redirectTo: 'login' },
	{ path: 'login', component: LoginComponent },
	{ path: 'register', component: RegisterComponent },
	{ path: 'forgot-password', component: ForgotpasswordComponent },
	{ path: 'reset-password', component: ResetpasswordComponent },
	{ path: 'dashboard', component: DashboardComponent, canActivate: [authGuard] },
	{ path: '**', redirectTo: 'login' },
];
