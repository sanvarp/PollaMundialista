import { DatePipe } from '@angular/common';
import { Component, computed, inject, input, linkedSignal, output, signal } from '@angular/core';
import { MatchesService } from '../../../core/api/matches.service';
import { MatchVm, MyPrediction } from '../../../core/models/match.models';

const MAX_GOALS = 30;

@Component({
  selector: 'app-match-card',
  imports: [DatePipe],
  templateUrl: './match-card.html',
  styleUrl: './match-card.scss',
})
export class MatchCard {
  private readonly matches = inject(MatchesService);

  readonly match = input.required<MatchVm>();
  readonly predictionSaved = output<{ matchId: number; prediction: MyPrediction }>();

  // Editable score, seeded from the existing prediction; resets if the match input changes.
  protected readonly home = linkedSignal(() => this.match().myPrediction?.homeGoals ?? 0);
  protected readonly away = linkedSignal(() => this.match().myPrediction?.awayGoals ?? 0);

  protected readonly saving = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly justSaved = signal(false);

  protected readonly hasResult = computed(() => this.match().result !== null);
  protected readonly locked = computed(() => this.match().isLocked);
  protected readonly editable = computed(() => !this.locked());

  protected readonly dirty = computed(() => {
    const p = this.match().myPrediction;
    return !p || p.homeGoals !== this.home() || p.awayGoals !== this.away();
  });

  step(side: 'home' | 'away', delta: number): void {
    const sig = side === 'home' ? this.home : this.away;
    sig.set(Math.max(0, Math.min(MAX_GOALS, sig() + delta)));
    this.justSaved.set(false);
  }

  save(): void {
    if (this.saving() || !this.editable()) return;
    this.saving.set(true);
    this.error.set(null);

    this.matches.upsertPrediction(this.match().id, { homeGoals: this.home(), awayGoals: this.away() }).subscribe({
      next: (prediction) => {
        this.saving.set(false);
        this.justSaved.set(true);
        this.predictionSaved.emit({ matchId: this.match().id, prediction });
      },
      error: (err) => {
        this.saving.set(false);
        this.error.set(err?.error?.detail ?? 'No se pudo guardar la predicción.');
      },
    });
  }
}
