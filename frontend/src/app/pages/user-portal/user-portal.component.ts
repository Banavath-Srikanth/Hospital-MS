import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { AuthService } from '../../services/auth.service';
import { environment } from '../../../environments/environment';

interface AppointmentSummary {
  id: number;
  doctorName: string;
  doctorSpecialization: string;
  appointmentDate: string;
  status: string;
}

@Component({
  selector: 'app-user-portal',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './user-portal.component.html',
  styleUrl: './user-portal.component.scss'
})
export class UserPortalComponent implements OnInit {
  user = signal<{ username: string; email: string; role: string; patientId?: number | null } | null>(null);
  upcoming  = signal<AppointmentSummary[]>([]);
  past      = signal<AppointmentSummary[]>([]);
  loading   = signal(true);

  constructor(private auth: AuthService, private http: HttpClient, private router: Router) {}

  ngOnInit() {
    this.user.set(this.auth.currentUser());
    this.loadAppointments();
  }

  private loadAppointments() {
    const token = this.auth.getToken();
    if (!token) return;

    const headers = new HttpHeaders({ Authorization: `Bearer ${token}` });
    this.http.get<any>(`${environment.apiUrl}/appointments/my`, { headers }).subscribe({
      next: (res) => {
        const all: AppointmentSummary[] = res.data ?? [];
        const now = new Date();
        this.upcoming.set(all.filter(a => a.status === 'Scheduled' && new Date(a.appointmentDate) >= now)
                             .sort((a, b) => new Date(a.appointmentDate).getTime() - new Date(b.appointmentDate).getTime()));
        this.past.set(all.filter(a => a.status !== 'Scheduled' || new Date(a.appointmentDate) < now)
                        .sort((a, b) => new Date(b.appointmentDate).getTime() - new Date(a.appointmentDate).getTime())
                        .slice(0, 3));
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  formatDate(dateStr: string): string {
    return new Date(dateStr).toLocaleDateString('en-IN', {
      day: 'numeric', month: 'short', year: 'numeric', hour: '2-digit', minute: '2-digit'
    });
  }

  getStatusClass(status: string): string {
    return { Scheduled: 'badge-scheduled', Completed: 'badge-completed', Cancelled: 'badge-cancelled' }[status] ?? '';
  }

  navigateTo(path: string) {
    this.router.navigate([path]);
  }
}
