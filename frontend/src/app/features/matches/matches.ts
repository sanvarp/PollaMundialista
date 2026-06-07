import { DatePipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { RevealOnScrollDirective } from '../../core/motion/reveal-on-scroll.directive';
import { SplitRevealDirective } from '../../core/motion/split-reveal.directive';
import { MatchesService } from '../../core/api/matches.service';
import { MatchVm, MyPrediction } from '../../core/models/match.models';
import { AppHeader } from '../../shared/ui/app-header/app-header';
import { MatchCard } from './match-card/match-card';

@Component({
  selector: 'app-matches',
  imports: [AppHeader, MatchCard, SplitRevealDirective, RevealOnScrollDirective, DatePipe],
  templateUrl: './matches.html',
  styleUrl: './matches.scss',
})
export class Matches {
  private readonly matchesApi = inject(MatchesService);

  protected readonly matches = signal<MatchVm[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  /** 'group' = by group letter · 'date' = chronological by kickoff (play order). */
  protected readonly view = signal<'group' | 'date'>('group');

  /** Matches grouped by group label, for sectioned rendering. */
  protected readonly groups = computed(() => {
    const byGroup = new Map<string, MatchVm[]>();
    for (const m of this.matches()) {
      (byGroup.get(m.group) ?? byGroup.set(m.group, []).get(m.group)!).push(m);
    }
    return [...byGroup.entries()].sort(([a], [b]) => a.localeCompare(b)).map(([group, matches]) => ({ group, matches }));
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

  protected readonly openCount = computed(() => this.matches().filter((m) => !m.isLocked).length);

  /** Of the open matches, how many the user has already predicted / has left. */
  protected readonly predictedOpen = computed(
    () => this.matches().filter((m) => !m.isLocked && m.myPrediction !== null).length,
  );
  protected readonly remainingOpen = computed(() => this.openCount() - this.predictedOpen());
  protected readonly progressPct = computed(() => {
    const total = this.openCount();
    return total === 0 ? 0 : Math.round((this.predictedOpen() / total) * 100);
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

  /** Patch the saved prediction back into local state so the card reflects it. */
  onPredictionSaved(event: { matchId: number; prediction: MyPrediction }): void {
    this.matches.update((list) =>
      list.map((m) => (m.id === event.matchId ? { ...m, myPrediction: event.prediction } : m)),
    );
  }
}
