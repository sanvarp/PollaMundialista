import { Component, OnInit, inject, input, signal } from '@angular/core';
import { LeaderboardService } from '../../core/api/leaderboard.service';
import { UserHistory } from '../../core/models/leaderboard.models';
import { AppHeader } from '../../shared/ui/app-header/app-header';
import { HistoryRow } from './history-row/history-row';

@Component({
  selector: 'app-user-history',
  imports: [AppHeader, HistoryRow],
  templateUrl: './user-history.html',
  styleUrl: './user-history.scss',
})
export class UserHistoryPage implements OnInit {
  private readonly api = inject(LeaderboardService);

  /** Bound from the :id route param (withComponentInputBinding). */
  readonly id = input.required<string>();

  protected readonly history = signal<UserHistory | null>(null);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  ngOnInit(): void {
    this.api.getUserHistory(this.id()).subscribe({
      next: (h) => {
        this.history.set(h);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(err?.status === 404 ? 'Usuario no encontrado.' : 'No se pudo cargar el historial.');
        this.loading.set(false);
      },
    });
  }
}
