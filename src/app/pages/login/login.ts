import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  protected readonly passwordVisible = signal(false);
  protected readonly rememberMe = signal(true);
  protected readonly languageOpen = signal(false);
  protected readonly message = signal('');

  protected emailOrUsername = '';
  protected password = '';

  protected togglePassword(): void {
    this.passwordVisible.update((value) => !value);
  }

  protected toggleRemember(): void {
    this.rememberMe.update((value) => !value);
  }

  protected submit(): void {
    this.message.set('');

    if (!this.emailOrUsername.trim() || !this.password.trim()) {
      this.message.set('Enter your email or username and password.');
      return;
    }

    this.message.set('Login form is ready for backend integration.');
  }

  protected socialLogin(provider: string): void {
    this.message.set(`${provider} sign-in is ready for backend integration.`);
  }
}
