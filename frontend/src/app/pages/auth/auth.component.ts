import { Component, signal } from '@angular/core';
import { CommonModule }       from '@angular/common';
import { FormsModule }        from '@angular/forms';
import { Router }             from '@angular/router';
import { AuthService }        from '../../services/auth.service';

@Component({
  selector: 'app-auth',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './auth.component.html',
  styleUrl: './auth.component.scss'
})
export class AuthComponent {
  loading  = signal(false);
  errorMsg = signal('');
  activeTab: 'login' | 'register' = 'login';

  // ── Login form ──────────────────────────────────────────────────────────────
  loginEmail    = '';
  loginPassword = '';
  showPassword  = false;
  loginErrors: Record<string, string> = {};

  // ── Register form ───────────────────────────────────────────────────────────
  regUsername        = '';
  regEmail           = '';
  regPassword        = '';
  regConfirmPassword = '';
  regPhone           = '';
  regAge: number | null = null;
  regGender          = '';
  showRegPassword    = false;
  registerErrors: Record<string, string> = {};

  constructor(private auth: AuthService, private router: Router) {}

  switchTab(tab: 'login' | 'register') {
    this.activeTab = tab;
    this.errorMsg.set('');
    this.loginErrors = {};
    this.registerErrors = {};
  }

  // ── Login ───────────────────────────────────────────────────────────────────
  onLogin() {
    this.loginErrors = {};
    this.errorMsg.set('');

    if (!this.loginEmail)                         this.loginErrors['email']    = 'Email is required.';
    else if (!this.isValidEmail(this.loginEmail)) this.loginErrors['email']    = 'Enter a valid email address.';
    if (!this.loginPassword)                      this.loginErrors['password'] = 'Password is required.';

    if (Object.keys(this.loginErrors).length) return;

    this.loading.set(true);
    this.auth.login(this.loginEmail, this.loginPassword).subscribe({
      next: () => {
        this.loading.set(false);
        // Redirect based on role
        if (this.auth.isPatient()) {
          this.router.navigate(['/user-portal']);
        } else {
          this.router.navigate(['/']);
        }
      },
      error: (err) => {
        this.loading.set(false);
        this.errorMsg.set(err.error?.message ?? 'Invalid email or password.');
      }
    });
  }

  // ── Register ────────────────────────────────────────────────────────────────
  onRegister() {
    this.registerErrors = {};
    this.errorMsg.set('');

    if (!this.regUsername)    this.registerErrors['username'] = 'Full name is required.';
    if (!this.regEmail)       this.registerErrors['email']    = 'Email is required.';
    else if (!this.isValidEmail(this.regEmail)) this.registerErrors['email'] = 'Enter a valid email address.';
    if (!this.regPassword)    this.registerErrors['password'] = 'Password is required.';
    else if (this.regPassword.length < 8) this.registerErrors['password'] = 'Password must be at least 8 characters.';
    if (this.regPassword !== this.regConfirmPassword) this.registerErrors['confirmPassword'] = 'Passwords do not match.';
    if (!this.regPhone)       this.registerErrors['phone']    = 'Phone number is required.';
    if (!this.regAge || this.regAge <= 0) this.registerErrors['age'] = 'Please enter a valid age.';
    if (!this.regGender)      this.registerErrors['gender']   = 'Please select your gender.';

    if (Object.keys(this.registerErrors).length) return;

    this.loading.set(true);
    this.auth.register({
      username:        this.regUsername,
      email:           this.regEmail,
      password:        this.regPassword,
      confirmPassword: this.regConfirmPassword,
      phoneNumber:     this.regPhone,
      age:             this.regAge!,
      gender:          this.regGender
    }).subscribe({
      next: () => {
        this.loading.set(false);
        this.router.navigate(['/user-portal']);
      },
      error: (err) => {
        this.loading.set(false);
        this.errorMsg.set(err.error?.message ?? 'Registration failed. Please try again.');
      }
    });
  }

  private isValidEmail(email: string): boolean {
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
  }
}
