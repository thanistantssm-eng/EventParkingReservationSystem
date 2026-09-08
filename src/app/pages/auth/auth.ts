import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

@Component({
  selector: 'app-auth',
  standalone: true,
  imports: [FormsModule, RouterLink],
  templateUrl: './auth.html',
  styleUrl: './auth.css',
})
export class Auth {
  protected readonly mode: 'login' | 'register' | 'reset';
  protected readonly loginTarget: string;
  protected readonly passwordVisible = signal(false);
  protected readonly remember = signal(true);
  protected readonly message = signal('');
  protected readonly resetStage = signal<'request' | 'change'>('request');
  protected readonly roleLabel: 'Customer' | 'Organizer' | 'Administrator';
  protected name = '';
  protected email = '';
  protected phone = '';
  protected password = '';
  protected confirm = '';
  protected code = '';
  constructor(private readonly router: Router) {
    this.mode = router.url.includes('register')
      ? 'register'
      : router.url.includes('reset') || router.url.includes('forgot-password')
        ? 'reset'
        : 'login';
    this.loginTarget = router.url.includes('role=admin')
      ? '/admin/dashboard'
      : router.url.includes('role=organizer')
        ? '/organizer/dashboard'
        : '/customer/dashboard';
    this.roleLabel = this.loginTarget.startsWith('/admin')
      ? 'Administrator'
      : this.loginTarget.startsWith('/organizer')
        ? 'Organizer'
        : 'Customer';
  }
  protected chooseLanguage(): void {
    this.message.set('English is selected. Tamil and Sinhala translations are coming soon.');
  }
  protected socialLogin(provider: string): void {
    const role = this.loginTarget.startsWith('/admin')
      ? 'admin'
      : this.loginTarget.startsWith('/organizer')
        ? 'organizer'
        : 'customer';
    try {
      sessionStorage.setItem('eventora-role', role);
    } catch {
      /* storage may be blocked in preview */
    }
    this.message.set(`Continuing with ${provider}…`);
    setTimeout(() => void this.router.navigateByUrl(this.loginTarget), 350);
  }
  protected submit(): void {
    if (this.mode === 'login') {
      if (!this.email || !this.password) {
        this.message.set('Enter your email and password.');
        return;
      }
      const role = this.loginTarget.startsWith('/admin')
        ? 'admin'
        : this.loginTarget.startsWith('/organizer')
          ? 'organizer'
          : 'customer';
      try {
        sessionStorage.setItem('eventora-role', role);
      } catch {
        /* storage may be blocked in preview */
      }
      void this.router.navigateByUrl(this.loginTarget);
    } else if (this.mode === 'register') {
      if (
        !this.name ||
        !this.email ||
        !this.phone ||
        !this.password ||
        this.password !== this.confirm
      ) {
        this.message.set('Complete all fields and ensure passwords match.');
        return;
      }
      try {
        sessionStorage.setItem('eventora-role', 'customer');
      } catch {
        /* storage may be blocked in preview */
      }
      this.message.set('Account created successfully. Redirecting…');
      setTimeout(() => void this.router.navigateByUrl('/customer/dashboard'), 700);
    } else {
      if (this.resetStage() === 'request') {
        if (!this.email) {
          this.message.set('Enter your registered email address.');
          return;
        }
        this.resetStage.set('change');
        this.message.set('Verification code sent. Use 123456 for this demo.');
        return;
      }
      if (this.code !== '123456') {
        this.message.set('Enter the demo verification code 123456.');
        return;
      }
      if (!this.password || this.password !== this.confirm) {
        this.message.set('Create a password and ensure both passwords match.');
        return;
      }
      this.message.set('Password reset successfully. Redirecting to sign in…');
      setTimeout(() => void this.router.navigateByUrl('/login'), 700);
    }
  }
}
