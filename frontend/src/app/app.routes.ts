import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';
import { roleGuard } from './guards/role.guard';

export const routes: Routes = [
  // ── Public ────────────────────────────────────────────────────────────────
  { path: 'login', loadComponent: () => import('./pages/auth/auth.component').then(m => m.AuthComponent), title: 'Login — HMS' },

  // ── Admin / Staff routes ───────────────────────────────────────────────────
  {
    path: '',
    loadComponent: () => import('./pages/dashboard/dashboard.component').then(m => m.DashboardComponent),
    title: 'Dashboard — HMS',
    canActivate: [authGuard, roleGuard('Admin', 'Staff')]
  },
  {
    path: 'patients',
    loadComponent: () => import('./pages/patients/patients.component').then(m => m.PatientsComponent),
    title: 'Patients — HMS',
    canActivate: [authGuard, roleGuard('Admin', 'Staff')]
  },
  {
    path: 'doctors',
    loadComponent: () => import('./pages/doctors/doctors.component').then(m => m.DoctorsComponent),
    title: 'Doctors — HMS',
    canActivate: [authGuard, roleGuard('Admin', 'Staff')]
  },
  {
    path: 'appointments',
    loadComponent: () => import('./pages/appointments/appointments.component').then(m => m.AppointmentsComponent),
    title: 'Appointments — HMS',
    canActivate: [authGuard, roleGuard('Admin', 'Staff')]
  },

  // ── Patient routes ────────────────────────────────────────────────────────
  {
    path: 'user-portal',
    loadComponent: () => import('./pages/user-portal/user-portal.component').then(m => m.UserPortalComponent),
    title: 'My Dashboard — HMS',
    canActivate: [authGuard, roleGuard('Patient')]
  },
  {
    path: 'book-appointment',
    loadComponent: () => import('./pages/book-appointment/book-appointment.component').then(m => m.BookAppointmentComponent),
    title: 'Book Appointment — HMS',
    canActivate: [authGuard, roleGuard('Patient')]
  },
  {
    path: 'my-appointments',
    loadComponent: () => import('./pages/my-appointments/my-appointments.component').then(m => m.MyAppointmentsComponent),
    title: 'My Appointments — HMS',
    canActivate: [authGuard, roleGuard('Patient')]
  },
  {
    path: 'doctors-list',
    loadComponent: () => import('./pages/doctors-list/doctors-list.component').then(m => m.DoctorsListComponent),
    title: 'Doctors — HMS',
    canActivate: [authGuard, roleGuard('Patient')]
  },

  { path: '**', redirectTo: '' }
];
