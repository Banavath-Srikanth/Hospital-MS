import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { AuthService } from '../../services/auth.service';
import { environment } from '../../../environments/environment';

interface Department { id: number; name: string; code: string; }
interface Doctor { id: number; fullName: string; specialization: string; payrollPosition: string; isAvailable: boolean; departmentId?: number; departmentName?: string; }

@Component({
  selector: 'app-book-appointment',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './book-appointment.component.html',
  styleUrl: './book-appointment.component.scss'
})
export class BookAppointmentComponent implements OnInit {
  step          = signal(1);   // 1=Department, 2=Doctor, 3=DateTime, 4=Confirm/Done
  loading       = signal(false);
  successMsg    = signal('');
  errorMsg      = signal('');

  departments   = signal<Department[]>([]);
  doctors       = signal<Doctor[]>([]);

  selectedDept: Department | null = null;
  selectedDoc:  Doctor | null     = null;
  selectedDate  = '';
  selectedTime  = '';
  minDate       = '';

  constructor(private auth: AuthService, private http: HttpClient, private router: Router) {}

  ngOnInit() {
    // Min date = tomorrow
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    this.minDate = tomorrow.toISOString().split('T')[0];

    this.loadDepartments();
  }

  private headers() {
    return new HttpHeaders({ Authorization: `Bearer ${this.auth.getToken()}` });
  }

  loadDepartments() {
    this.http.get<any>(`${environment.apiUrl}/departments`, { headers: this.headers() }).subscribe({
      next: res => this.departments.set(res.data ?? res),
      error: () => {}
    });
  }

  selectDepartment(dept: Department) {
    this.selectedDept = dept;
    this.loadDoctorsByDept(dept.id);
    this.step.set(2);
  }

  loadDoctorsByDept(deptId: number) {
    this.http.get<any>(`${environment.apiUrl}/doctors/department/${deptId}`, { headers: this.headers() }).subscribe({
      next: res => {
        const all: Doctor[] = res.data ?? res;
        this.doctors.set(all.filter(d => d.isAvailable));
      },
      error: () => {}
    });
  }

  selectDoctor(doc: Doctor) {
    this.selectedDoc = doc;
    this.step.set(3);
  }

  goToStep(n: number) {
    if (n < this.step()) this.step.set(n);
  }

  proceedToConfirm() {
    this.errorMsg.set('');
    if (!this.selectedDate) { this.errorMsg.set('Please select a date.'); return; }
    if (!this.selectedTime) { this.errorMsg.set('Please select a time.'); return; }
    this.step.set(4);
  }

  get appointmentDateTime(): Date {
    return new Date(`${this.selectedDate}T${this.selectedTime}:00`);
  }

  /**
   * Returns a local ISO string (YYYY-MM-DDTHH:mm:ss) without UTC conversion.
   * Using toISOString() would shift the time to UTC (e.g. 2:30 PM IST → 9:00 AM UTC),
   * which causes the stored time to differ from what the user selected.
   */
  private toLocalISOString(date: Date): string {
    const pad = (n: number) => String(n).padStart(2, '0');
    return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}` +
           `T${pad(date.getHours())}:${pad(date.getMinutes())}:${pad(date.getSeconds())}`;
  }

  confirm() {
    this.loading.set(true);
    this.errorMsg.set('');

    const body = {
      patientId:       0,   // server overrides with JWT patientId
      doctorId:        this.selectedDoc!.id,
      appointmentDate: this.toLocalISOString(this.appointmentDateTime)
    };

    this.http.post<any>(`${environment.apiUrl}/appointments`, body, { headers: this.headers() }).subscribe({
      next: () => {
        this.loading.set(false);
        this.successMsg.set('Appointment booked successfully! 🎉');
        this.step.set(5);
      },
      error: (err) => {
        this.loading.set(false);
        this.errorMsg.set(err.error?.message ?? 'Failed to book appointment.');
        this.step.set(4);
      }
    });
  }

  formatDate(d: Date): string {
    return d.toLocaleDateString('en-IN', { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' });
  }

  formatTime(time: string): string {
    const [h, m] = time.split(':');
    const date = new Date();
    date.setHours(parseInt(h), parseInt(m));
    return date.toLocaleTimeString('en-IN', { hour: '2-digit', minute: '2-digit', hour12: true });
  }

  // Available time slots
  timeSlots = [
    '09:00','09:30','10:00','10:30','11:00','11:30',
    '12:00','14:00','14:30','15:00','15:30','16:00','16:30','17:00'
  ];
}
