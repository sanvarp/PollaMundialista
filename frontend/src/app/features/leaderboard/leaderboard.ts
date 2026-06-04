import { Component, DestroyRef, ElementRef, computed, inject, signal, viewChild } from '@angular/core';
import { Router } from '@angular/router';
import { LeaderboardService } from '../../core/api/leaderboard.service';
import { AuthService } from '../../core/auth/auth.service';
import { CountUpDirective } from '../../core/motion/count-up.directive';
import { Flip, gsap, reducedMotion, registerGsap } from '../../core/motion/motion';
import { SplitRevealDirective } from '../../core/motion/split-reveal.directive';
import { LeaderboardEntry } from '../../core/models/leaderboard.models';
import { AppHeader } from '../../shared/ui/app-header/app-header';

const POLL_MS = 12_000;

@Component({
  selector: 'app-leaderboard',
  imports: [AppHeader, CountUpDirective, SplitRevealDirective],
  templateUrl: './leaderboard.html',
  styleUrl: './leaderboard.scss',
})
export class Leaderboard {
  private readonly api = inject(LeaderboardService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  private readonly board = viewChild<ElementRef<HTMLElement>>('board');

  protected readonly entries = signal<LeaderboardEntry[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  protected readonly myId = this.auth.user()?.userId ?? '';

  /** Top 3 reordered for a podium: 2nd, 1st, 3rd. */
  protected readonly podium = computed(() => {
    const t = this.entries().slice(0, 3);
    return [t[1], t[0], t[2]].filter(Boolean) as LeaderboardEntry[];
  });
  protected readonly rest = computed(() => this.entries().slice(3));

  constructor() {
    registerGsap();
    this.load(true);

    // Poll so a result posted by an admin animates in (Flip) without a manual refresh.
    const timer = setInterval(() => this.load(false), POLL_MS);
    inject(DestroyRef).onDestroy(() => clearInterval(timer));
  }

  private load(initial: boolean): void {
    if (initial) this.loading.set(true);

    // Capture positions before the data changes, to animate the reorder.
    const container = this.board()?.nativeElement;
    const flipState =
      !initial && !reducedMotion() && container
        ? Flip.getState(container.querySelectorAll('[data-flip-id]'))
        : null;

    this.api.getLeaderboard().subscribe({
      next: (e) => {
        const changed = JSON.stringify(e.map((x) => x.userId)) !== JSON.stringify(this.entries().map((x) => x.userId));
        this.entries.set(e);
        this.loading.set(false);
        if (flipState && changed) {
          // Wait for the DOM to reflect the new order, then animate from old positions.
          requestAnimationFrame(() =>
            requestAnimationFrame(() =>
              Flip.from(flipState, { duration: 0.7, ease: 'power2.inOut', absolute: true, stagger: 0.04 }),
            ),
          );
        }
      },
      error: () => {
        this.error.set('No se pudo cargar la tabla.');
        this.loading.set(false);
      },
    });
  }

  refresh(): void {
    this.load(false);
  }

  openUser(userId: string): void {
    this.router.navigate(['/users', userId]);
  }
}
