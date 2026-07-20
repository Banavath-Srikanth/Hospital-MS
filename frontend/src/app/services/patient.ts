import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../environments/environment';

// ── Matches PatientResponseDto from the backend ──────────────────────────────
export interface Patient {
  id: number;
  fullName: string;
  age: number;
  gender: string;
  disease: string;
  phoneNumber: string;
  admissionDate: string;
  isActive: boolean;
  totalAppointments: number;
}

// ── Matches CreatePatientDto from the backend ─────────────────────────────────
export interface CreatePatientDto {
  fullName: string;
  age: number;
  gender: string;
  disease: string;
  phoneNumber: string;
}

// ── Matches UpdatePatientDto from the backend ─────────────────────────────────
export interface UpdatePatientDto {
  fullName: string;
  age: number;
  gender: string;
  disease: string;
  phoneNumber: string;
  isActive: boolean;
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
export class PatientService {
  private url = `${environment.apiUrl}/patients`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Patient[]> {
    return this.http
      .get<ApiListResponse<Patient>>(this.url)
      .pipe(map(res => res.data));
  }

  getById(id: number): Observable<Patient> {
    return this.http
      .get<ApiSingleResponse<Patient>>(`${this.url}/${id}`)
      .pipe(map(res => res.data));
  }

  create(dto: CreatePatientDto): Observable<Patient> {
    return this.http
      .post<ApiSingleResponse<Patient>>(this.url, dto)
      .pipe(map(res => res.data));
  }

  update(id: number, dto: UpdatePatientDto): Observable<Patient> {
    return this.http
      .put<ApiSingleResponse<Patient>>(`${this.url}/${id}`, dto)
      .pipe(map(res => res.data));
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}
