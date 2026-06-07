import { DatePipe } from '@angular/common';
import { Component, computed, inject, input, linkedSignal, output, signal } from '@angular/core';
import { MatchesService } from '../../../core/api/matches.service';
import { Clock } from '../../../core/clock';
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
  private readonly clock = inject(Clock);

  readonly match = input.required<MatchVm>();
  readonly predictionSaved = output<{ matchId: number; prediction: MyPrediction }>();

  /** Live "closes in Xd Yh" for open matches; null once locked. */
  protected readonly countdown = computed(() => {
    if (this.match().isLocked) return null;
    const ms = new Date(this.match().kickoffUtc).getTime() - this.clock.now();
    if (ms <= 0) return null;
    const d = Math.floor(ms / 86_400_000);
    const h = Math.floor((ms % 86_400_000) / 3_600_000);
    const m = Math.floor((ms % 3_600_000) / 60_000);
    return d > 0 ? `${d}d ${h}h` : h > 0 ? `${h}h ${m}m` : `${m}m`;
  });

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

  /** Keyboard entry: keep only digits, clamp 0..MAX, keep the field in sync. */
  onType(side: 'home' | 'away', event: Event): void {
    const input = event.target as HTMLInputElement;
    const digits = input.value.replace(/\D/g, '');
    const clamped =
      digits === '' ? 0 : Math.max(0, Math.min(MAX_GOALS, Number.parseInt(digits, 10)));
    (side === 'home' ? this.home : this.away).set(clamped);
    const display = digits === '' ? '' : String(clamped);
    if (input.value !== display) input.value = display; // strip '-', letters, or > 30
    this.justSaved.set(false);
  }

  /** Up/down arrows step the score (this is a text input, not number). */
  onKeydown(side: 'home' | 'away', event: KeyboardEvent): void {
    if (event.key === 'ArrowUp') {
      event.preventDefault();
      this.step(side, 1);
    } else if (event.key === 'ArrowDown') {
      event.preventDefault();
      this.step(side, -1);
    }
  }

  selectOnFocus(event: FocusEvent): void {
    (event.target as HTMLInputElement).select();
  }

  save(): void {
    if (this.saving() || !this.editable()) return;
    this.saving.set(true);
    this.error.set(null);

    this.matches
      .upsertPrediction(this.match().id, { homeGoals: this.home(), awayGoals: this.away() })
      .subscribe({
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
