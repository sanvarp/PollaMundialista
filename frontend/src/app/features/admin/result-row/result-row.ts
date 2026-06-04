import { DatePipe } from '@angular/common';
import { Component, computed, inject, input, linkedSignal, output, signal } from '@angular/core';
import { AdminService } from '../../../core/api/admin.service';
import { MatchVm } from '../../../core/models/match.models';

const MAX_GOALS = 30;

@Component({
  selector: 'app-result-row',
  imports: [DatePipe],
  templateUrl: './result-row.html',
  styleUrl: './result-row.scss',
})
export class ResultRow {
  private readonly admin = inject(AdminService);

  readonly match = input.required<MatchVm>();
  readonly resultSaved = output<MatchVm>();

  protected readonly home = linkedSignal(() => this.match().result?.homeGoals ?? 0);
  protected readonly away = linkedSignal(() => this.match().result?.awayGoals ?? 0);

  protected readonly saving = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly justSaved = signal(false);

  protected readonly isFinished = computed(() => this.match().status === 'Finished');

  step(side: 'home' | 'away', delta: number): void {
    const sig = side === 'home' ? this.home : this.away;
    sig.set(Math.max(0, Math.min(MAX_GOALS, sig() + delta)));
    this.justSaved.set(false);
  }

  save(): void {
    if (this.saving()) return;
    this.saving.set(true);
    this.error.set(null);

    this.admin.setResult(this.match().id, { homeGoals: this.home(), awayGoals: this.away() }).subscribe({
      next: (updated) => {
        this.saving.set(false);
        this.justSaved.set(true);
        this.resultSaved.emit(updated);
      },
      error: (err) => {
        this.saving.set(false);
        this.error.set(err?.error?.detail ?? 'No se pudo guardar el resultado.');
      },
    });
  }
}
