import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-admin',
  imports: [RouterLink],
  template: `
    <section class="placeholder">
      <p class="placeholder__eyebrow">Admin</p>
      <h1>Panel de administración</h1>
      <p class="placeholder__note">La carga de resultados y el recálculo de puntajes llegan en M4.</p>
      <a routerLink="/">← Volver</a>
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
        font-size: clamp(2rem, 6vw, 3rem);
        margin: 0.5rem 0 1rem;
      }
      .placeholder__note {
        color: var(--text-secondary);
      }
      a {
        color: var(--accent);
        text-decoration: none;
      }
    `,
  ],
})
export class Admin {}
