import { Component, computed, inject, signal } from '@angular/core';
import { MatchesService } from '../../core/api/matches.service';
import { MatchVm } from '../../core/models/match.models';
import { AppHeader } from '../../shared/ui/app-header/app-header';
import { ResultRow } from './result-row/result-row';

@Component({
  selector: 'app-admin',
  imports: [AppHeader, ResultRow],
  templateUrl: './admin.html',
  styleUrl: './admin.scss',
})
export class Admin {
  private readonly matchesApi = inject(MatchesService);

  protected readonly matches = signal<MatchVm[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  protected readonly pendingCount = computed(
    () => this.matches().filter((m) => m.status !== 'Finished').length,
  );
  protected readonly finishedCount = computed(
    () => this.matches().filter((m) => m.status === 'Finished').length,
  );

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

  onResultSaved(updated: MatchVm): void {
    this.matches.update((list) =>
      list.map((m) => (m.id === updated.id ? { ...m, ...updated } : m)),
    );
  }
}
