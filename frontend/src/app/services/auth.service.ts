import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';

export interface AuthResponse {
  token: string;
  username: string;
  email: string;
  role: string;
  expiresAt: string;
  patientId?: number | null;
}

export interface RegisterPayload {
  username: string;
  email: string;
  password: string;
  confirmPassword: string;
  phoneNumber?: string;
  age?: number;
  gender?: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly apiUrl   = `${environment.apiUrl}/auth`;
  private readonly TOKEN_KEY = 'hms_token';
  private readonly USER_KEY  = 'hms_user';

  constructor(private http: HttpClient) {}

  // ── Register (Patient) ─────────────────────────────────────────────────────
  register(payload: RegisterPayload): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, payload)
      .pipe(tap(res => this.storeSession(res)));
  }

  // ── Login ──────────────────────────────────────────────────────────────────
  login(email: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, { email, password })
      .pipe(tap(res => this.storeSession(res)));
  }

  // ── Session helpers ────────────────────────────────────────────────────────
  private storeSession(res: AuthResponse): void {
    localStorage.setItem(this.TOKEN_KEY, res.token);
    localStorage.setItem(this.USER_KEY, JSON.stringify({
      username:  res.username,
      email:     res.email,
      role:      res.role,
      patientId: res.patientId ?? null
    }));
  }

  logout(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  /** Decodes the JWT payload and checks if it has expired. */
  private isTokenExpired(token: string): boolean {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      // exp is in seconds (Unix time)
      return Date.now() >= payload.exp * 1000;
    } catch {
      return true; // treat malformed tokens as expired
    }
  }

  isLoggedIn(): boolean {
    const token = this.getToken();
    if (!token) return false;
    if (this.isTokenExpired(token)) {
      // Clean up stale session automatically
      this.logout();
      return false;
    }
    return true;
  }

  currentUser(): { username: string; email: string; role: string; patientId?: number | null } | null {
    const raw = localStorage.getItem(this.USER_KEY);
    return raw ? JSON.parse(raw) : null;
  }

  // ── Role helpers ───────────────────────────────────────────────────────────
  isAdmin(): boolean {
    return this.currentUser()?.role === 'Admin';
  }

  isStaff(): boolean {
    return this.currentUser()?.role === 'Staff';
  }

  isPatient(): boolean {
    return this.currentUser()?.role === 'Patient';
  }

  isAdminOrStaff(): boolean {
    const role = this.currentUser()?.role;
    return role === 'Admin' || role === 'Staff';
  }

  getPatientId(): number | null {
    return this.currentUser()?.patientId ?? null;
  }
}
