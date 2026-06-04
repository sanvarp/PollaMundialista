import { Component, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { LeaderboardService } from '../../core/api/leaderboard.service';
import { AuthService } from '../../core/auth/auth.service';
import { LeaderboardEntry } from '../../core/models/leaderboard.models';
import { AppHeader } from '../../shared/ui/app-header/app-header';

@Component({
  selector: 'app-leaderboard',
  imports: [AppHeader],
  templateUrl: './leaderboard.html',
  styleUrl: './leaderboard.scss',
})
export class Leaderboard {
  private readonly api = inject(LeaderboardService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

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
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.api.getLeaderboard().subscribe({
      next: (e) => {
        this.entries.set(e);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('No se pudo cargar la tabla.');
        this.loading.set(false);
      },
    });
  }

  openUser(userId: string): void {
    this.router.navigate(['/users', userId]);
  }
}
