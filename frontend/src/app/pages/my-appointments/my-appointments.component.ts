import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { AuthService } from '../../services/auth.service';
import { environment } from '../../../environments/environment';

interface Appointment {
  id: number;
  doctorId: number;
  doctorName: string;
  doctorSpecialization: string;
  appointmentDate: string;
  status: string;
}

@Component({
  selector: 'app-my-appointments',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './my-appointments.component.html',
  styleUrl: './my-appointments.component.scss'
})
export class MyAppointmentsComponent implements OnInit {
  appointments = signal<Appointment[]>([]);
  loading      = signal(true);
  cancellingId = signal<number | null>(null);
  successMsg   = signal('');
  errorMsg     = signal('');
  filterStatus = signal<'all' | 'Scheduled' | 'Completed' | 'Cancelled'>('all');

  constructor(private auth: AuthService, private http: HttpClient) {}

  ngOnInit() {
    this.loadAppointments();
  }

  private headers() {
    return new HttpHeaders({ Authorization: `Bearer ${this.auth.getToken()}` });
  }

  loadAppointments() {
    this.loading.set(true);
    this.http.get<any>(`${environment.apiUrl}/appointments/my`, { headers: this.headers() }).subscribe({
      next: res => {
        const all: Appointment[] = res.data ?? [];
        // Sort newest first
        all.sort((a, b) => new Date(b.appointmentDate).getTime() - new Date(a.appointmentDate).getTime());
        this.appointments.set(all);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  get filtered(): Appointment[] {
    const f = this.filterStatus();
    if (f === 'all') return this.appointments();
    return this.appointments().filter(a => a.status === f);
  }

  setFilter(f: 'all' | 'Scheduled' | 'Completed' | 'Cancelled') {
    this.filterStatus.set(f);
  }

  cancelAppointment(id: number) {
    this.cancellingId.set(id);
    this.successMsg.set('');
    this.errorMsg.set('');

    this.http.patch<any>(`${environment.apiUrl}/appointments/${id}/cancel`, {}, { headers: this.headers() }).subscribe({
      next: () => {
        this.cancellingId.set(null);
        this.successMsg.set('Appointment cancelled successfully.');
        this.loadAppointments();
      },
      error: (err) => {
        this.cancellingId.set(null);
        this.errorMsg.set(err.error?.message ?? 'Failed to cancel appointment.');
      }
    });
  }

  isUpcoming(dateStr: string): boolean {
    return new Date(dateStr) >= new Date();
  }

  formatDate(dateStr: string): string {
    return new Date(dateStr).toLocaleDateString('en-IN', {
      weekday: 'short', day: 'numeric', month: 'short', year: 'numeric'
    });
  }

  formatTime(dateStr: string): string {
    return new Date(dateStr).toLocaleTimeString('en-IN', { hour: '2-digit', minute: '2-digit', hour12: true });
  }

  getStatusClass(status: string): string {
    return { Scheduled: 'badge-scheduled', Completed: 'badge-completed', Cancelled: 'badge-cancelled' }[status] ?? '';
  }

  countByStatus(status: string): number {
    return this.appointments().filter(a => a.status === status).length;
  }
}
