import { Component, computed, inject, signal } from '@angular/core';
import { MatchesService } from '../../core/api/matches.service';
import { MatchVm, MyPrediction } from '../../core/models/match.models';
import { AppHeader } from '../../shared/ui/app-header/app-header';
import { MatchCard } from './match-card/match-card';

@Component({
  selector: 'app-matches',
  imports: [AppHeader, MatchCard],
  templateUrl: './matches.html',
  styleUrl: './matches.scss',
})
export class Matches {
  private readonly matchesApi = inject(MatchesService);

  protected readonly matches = signal<MatchVm[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  /** Matches grouped by group label, for sectioned rendering. */
  protected readonly groups = computed(() => {
    const byGroup = new Map<string, MatchVm[]>();
    for (const m of this.matches()) {
      (byGroup.get(m.group) ?? byGroup.set(m.group, []).get(m.group)!).push(m);
    }
    return [...byGroup.entries()].sort(([a], [b]) => a.localeCompare(b)).map(([group, matches]) => ({ group, matches }));
  });

  protected readonly openCount = computed(() => this.matches().filter((m) => !m.isLocked).length);

  constructor() {
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.matchesApi.getMatches().subscribe({
      next: (m) => {
        this.matches.set(m);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('No se pudieron cargar los partidos.');
        this.loading.set(false);
      },
    });
  }

  /** Patch the saved prediction back into local state so the card reflects it. */
  onPredictionSaved(event: { matchId: number; prediction: MyPrediction }): void {
    this.matches.update((list) =>
      list.map((m) => (m.id === event.matchId ? { ...m, myPrediction: event.prediction } : m)),
    );
  }
}
