import { DatePipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { MatchesService } from '../../core/api/matches.service';
import { MatchVm } from '../../core/models/match.models';
import { AppHeader } from '../../shared/ui/app-header/app-header';
import { ResultRow } from './result-row/result-row';

@Component({
  selector: 'app-admin',
  imports: [AppHeader, ResultRow, DatePipe],
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

  /** 'group' = by group letter · 'date' = chronological by kickoff. */
  protected readonly view = signal<'group' | 'date'>('group');

  /** Matches grouped by group label, for sectioned rendering. */
  protected readonly groups = computed(() => {
    const byGroup = new Map<string, MatchVm[]>();
    for (const m of this.matches()) {
      (byGroup.get(m.group) ?? byGroup.set(m.group, []).get(m.group)!).push(m);
    }
    return [...byGroup.entries()]
      .sort(([a], [b]) => a.localeCompare(b))
      .map(([group, matches]) => ({ group, matches }));
  });

  /** Matches grouped by calendar day (local), in kickoff order. */
  protected readonly days = computed(() => {
    const sorted = [...this.matches()].sort((a, b) => a.kickoffUtc.localeCompare(b.kickoffUtc));
    const days: { key: string; date: string; matches: MatchVm[] }[] = [];
    for (const m of sorted) {
      const d = new Date(m.kickoffUtc);
      const key = `${d.getFullYear()}-${d.getMonth()}-${d.getDate()}`;
      let day = days.find((x) => x.key === key);
      if (!day) {
        day = { key, date: m.kickoffUtc, matches: [] };
        days.push(day);
      }
      day.matches.push(m);
    }
    return days;
  });

  setView(mode: 'group' | 'date'): void {
    this.view.set(mode);
  }

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
