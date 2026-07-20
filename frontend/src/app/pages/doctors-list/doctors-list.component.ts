import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { AuthService } from '../../services/auth.service';
import { environment } from '../../../environments/environment';

interface Doctor {
  id: number;
  badgeId: string;
  fullName: string;
  specialization: string;
  payrollPosition: string;
  phoneNumber: string;
  email: string;
  isAvailable: boolean;
  departmentId?: number;
  departmentName?: string;
}

@Component({
  selector: 'app-doctors-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './doctors-list.component.html',
  styleUrl: './doctors-list.component.scss'
})
export class DoctorsListComponent implements OnInit {
  allDoctors  = signal<Doctor[]>([]);
  loading     = signal(true);
  searchQuery = signal('');
  filterAvail = signal<boolean | null>(null);
  filterSpec  = signal('');

  specializations = signal<string[]>([]);

  filtered = computed(() => {
    const q    = this.searchQuery().toLowerCase();
    const avail = this.filterAvail();
    const spec  = this.filterSpec().toLowerCase();

    return this.allDoctors().filter(d => {
      const matchQ    = !q || d.fullName.toLowerCase().includes(q) || d.specialization.toLowerCase().includes(q);
      const matchAvail = avail === null || d.isAvailable === avail;
      const matchSpec  = !spec || d.specialization.toLowerCase().includes(spec);
      return matchQ && matchAvail && matchSpec;
    });
  });

  constructor(private auth: AuthService, private http: HttpClient) {}

  ngOnInit() {
    const headers = new HttpHeaders({ Authorization: `Bearer ${this.auth.getToken()}` });
    this.http.get<any>(`${environment.apiUrl}/doctors`, { headers }).subscribe({
      next: res => {
        const docs: Doctor[] = res.data ?? res;
        this.allDoctors.set(docs);
        const specs = [...new Set(docs.map(d => d.specialization))].sort();
        this.specializations.set(specs);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  setAvailFilter(v: boolean | null) { this.filterAvail.set(v); }

  getSpecIcon(spec: string): string {
    const map: Record<string, string> = {
      'Cardiology': '❤️', 'Neurology': '🧠', 'Orthopedics': '🦴',
      'General': '🩺', 'Dermatology': '🧬', 'Ophthalmology': '👁️'
    };
    return map[spec] ?? '🏥';
  }
}
