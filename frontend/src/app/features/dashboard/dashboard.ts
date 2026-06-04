import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-dashboard',
  imports: [RouterLink],
  template: `
    <section class="placeholder">
      <p class="placeholder__eyebrow">Polla Mundialista</p>
      <h1>Hola, {{ auth.displayName() }} 👋</h1>
      <p class="placeholder__note">
        Sesión iniciada como <strong>{{ auth.user()?.role }}</strong>.
        Los partidos y predicciones llegan en M3.
      </p>
      <nav class="placeholder__nav">
        @if (auth.isAdmin()) {
          <a routerLink="/admin">Panel admin</a>
        }
        <button type="button" (click)="auth.logout()" routerLink="/login">Salir</button>
      </nav>
    </section>
  `,
  styles: [
    `
      .placeholder {
        max-width: 40rem;
        margin: 0 auto;
        padding: 6rem 1.5rem;
      }
      .placeholder__eyebrow {
        font-family: var(--font-mono);
        font-size: 0.7rem;
        letter-spacing: 0.18em;
        text-transform: uppercase;
        color: var(--accent);
      }
      h1 {
        font-size: clamp(2.25rem, 7vw, 3.5rem);
        letter-spacing: -0.02em;
        margin: 0.5rem 0 1rem;
      }
      .placeholder__note {
        color: var(--text-secondary);
      }
      .placeholder__nav {
        display: flex;
        gap: 1rem;
        margin-top: 2rem;
        align-items: center;
      }
      a,
      button {
        color: var(--accent);
        background: none;
        border: 1px solid var(--border);
        border-radius: 10px;
        padding: 0.6rem 1rem;
        font: inherit;
        cursor: pointer;
        text-decoration: none;
      }
    `,
  ],
})
export class Dashboard {
  protected readonly auth = inject(AuthService);
}
