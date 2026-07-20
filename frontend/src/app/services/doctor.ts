import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../environments/environment';

// ── Matches DoctorResponseDto from the backend ────────────────────────────────
export interface Doctor {
  id: number;
  badgeId: string;
  fullName: string;
  specialization: string;
  payrollPosition: string;
  phoneNumber: string;
  email: string;
  isAvailable: boolean;
  totalAppointments: number;
  departmentId: number | null;
  departmentName: string;
}

// ── Matches CreateDoctorDto from the backend ──────────────────────────────────
export interface CreateDoctorDto {
  fullName: string;
  specialization: string;
  payrollPosition: string;
  phoneNumber: string;
  email: string;
  departmentId: number;
}

// ── Matches UpdateDoctorDto from the backend ──────────────────────────────────
export interface UpdateDoctorDto {
  fullName: string;
  specialization: string;
  payrollPosition: string;
  phoneNumber: string;
  email: string;
  isAvailable: boolean;
  departmentId: number;
}

// ── Department ─────────────────────────────────────────────────────────────────
export interface Department {
  id: number;
  name: string;
  code: string;
  doctorCount: number;
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
export class DoctorService {
  private url = `${environment.apiUrl}/doctors`;
  private deptUrl = `${environment.apiUrl}/departments`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Doctor[]> {
    return this.http
      .get<ApiListResponse<Doctor>>(this.url)
      .pipe(map(res => res.data));
  }

  getById(id: number): Observable<Doctor> {
    return this.http
      .get<ApiSingleResponse<Doctor>>(`${this.url}/${id}`)
      .pipe(map(res => res.data));
  }

  getAvailable(): Observable<Doctor[]> {
    return this.http
      .get<ApiListResponse<Doctor>>(`${this.url}/available`)
      .pipe(map(res => res.data));
  }

  getByDepartment(departmentId: number): Observable<Doctor[]> {
    return this.http
      .get<ApiListResponse<Doctor>>(`${this.url}/department/${departmentId}`)
      .pipe(map(res => res.data));
  }

  getDepartments(): Observable<Department[]> {
    return this.http
      .get<ApiListResponse<Department>>(this.deptUrl)
      .pipe(map(res => res.data));
  }

  create(dto: CreateDoctorDto): Observable<Doctor> {
    return this.http
      .post<ApiSingleResponse<Doctor>>(this.url, dto)
      .pipe(map(res => res.data));
  }

  update(id: number, dto: UpdateDoctorDto): Observable<Doctor> {
    return this.http
      .put<ApiSingleResponse<Doctor>>(`${this.url}/${id}`, dto)
      .pipe(map(res => res.data));
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}
