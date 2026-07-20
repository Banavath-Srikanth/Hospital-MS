import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../environments/environment';

// ── Matches AppointmentResponseDto from the backend ───────────────────────────
export interface Appointment {
  id: number;
  patientId: number;
  patientName: string;
  doctorId: number;
  doctorName: string;
  doctorSpecialization: string;
  appointmentDate: string;
  status: string;
}

// ── Matches CreateAppointmentDto from the backend ─────────────────────────────
export interface CreateAppointmentDto {
  patientId: number;
  doctorId: number;
  appointmentDate: string;
}

// ── Matches UpdateAppointmentDto from the backend ─────────────────────────────
export interface UpdateAppointmentDto {
  appointmentDate: string;
  status: string;
}

// ── API envelope shape ────────────────────────────────────────────────────────
interface ApiListResponse<T> {
  success: boolean;
  count: number;
  data: T[];
}

interface ApiSingleResponse<T> {
  success: boolean;
  data: T;
}

@Injectable({ providedIn: 'root' })
export class AppointmentService {
  private url = `${environment.apiUrl}/appointments`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Appointment[]> {
    return this.http
      .get<ApiListResponse<Appointment>>(this.url)
      .pipe(map(res => res.data));
  }

  getById(id: number): Observable<Appointment> {
    return this.http
      .get<ApiSingleResponse<Appointment>>(`${this.url}/${id}`)
      .pipe(map(res => res.data));
  }

  getByPatient(patientId: number): Observable<Appointment[]> {
    return this.http
      .get<ApiListResponse<Appointment>>(`${this.url}/patient/${patientId}`)
      .pipe(map(res => res.data));
  }

  getByDoctor(doctorId: number): Observable<Appointment[]> {
    return this.http
      .get<ApiListResponse<Appointment>>(`${this.url}/doctor/${doctorId}`)
      .pipe(map(res => res.data));
  }

  create(dto: CreateAppointmentDto): Observable<Appointment> {
    return this.http
      .post<ApiSingleResponse<Appointment>>(this.url, dto)
      .pipe(map(res => res.data));
  }

  update(id: number, dto: UpdateAppointmentDto): Observable<Appointment> {
    return this.http
      .put<ApiSingleResponse<Appointment>>(`${this.url}/${id}`, dto)
      .pipe(map(res => res.data));
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}
