import { Component, computed, inject, signal } from '@angular/core';
import { StandingsService } from '../../core/api/standings.service';
import { GroupStanding } from '../../core/models/standings.models';
import { SplitRevealDirective } from '../../core/motion/split-reveal.directive';
import { AppHeader } from '../../shared/ui/app-header/app-header';

@Component({
  selector: 'app-standings',
  imports: [AppHeader, SplitRevealDirective],
  templateUrl: './standings.html',
  styleUrl: './standings.scss',
})
export class Standings {
  private readonly api = inject(StandingsService);

  protected readonly groups = signal<GroupStanding[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  protected readonly anyPlayed = computed(() =>
    this.groups().some((g) => g.rows.some((r) => r.played > 0)),
  );

  constructor() {
    this.api.getStandings().subscribe({
      next: (g) => {
        this.groups.set(g);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('No se pudieron cargar las tablas.');
        this.loading.set(false);
      },
    });
  }
}
