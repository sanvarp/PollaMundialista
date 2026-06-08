import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { SplitRevealDirective } from '../../../core/motion/split-reveal.directive';
import { EMAIL_PATTERN } from '../../../core/validators';
import { ShaderHero } from '../../../shared/ui/shader-hero/shader-hero';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, RouterLink, ShaderHero, SplitRevealDirective],
  templateUrl: './login.html',
  styleUrl: './auth.scss',
})
export class Login {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly reveal = signal(false);

  protected readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.pattern(EMAIL_PATTERN)]],
    password: ['', [Validators.required]],
  });

  submit(): void {
    if (this.form.invalid || this.loading()) {
      this.form.markAllAsTouched();
      return;
    }
    this.loading.set(true);
    this.error.set(null);

    this.auth.login(this.form.getRawValue()).subscribe({
      next: () => {
        this.loading.set(false);
        const raw = new URLSearchParams(location.search).get('returnUrl') ?? '';
        const path = raw.split(/[?#]/)[0];
        // Only follow a safe internal path. A returnUrl whose path is an auth page (e.g.
        // nested from repeated guard redirects) would re-enter this component and freeze the
        // button on "Entrando…"; an external/protocol-relative/backslash one would be an open
        // redirect. Fall back to home in those cases.
        const safe =
          raw.startsWith('/') &&
          !raw.startsWith('//') &&
          !raw.includes('\\') &&
          path !== '/login' &&
          path !== '/register';
        this.router.navigateByUrl(safe ? raw : '/');
      },
      error: (err) => {
        this.error.set(err?.error?.detail ?? 'No fue posible iniciar sesión. Verifica tus datos.');
        this.loading.set(false);
      },
    });
  }
}
