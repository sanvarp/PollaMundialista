import { DatePipe } from '@angular/common';
import { Component, computed, input } from '@angular/core';
import { UserHistoryEntry } from '../../../core/models/leaderboard.models';

@Component({
  selector: 'app-history-row',
  imports: [DatePipe],
  templateUrl: './history-row.html',
  styleUrl: './history-row.scss',
})
export class HistoryRow {
  readonly entry = input.required<UserHistoryEntry>();

  protected readonly played = computed(() => this.entry().result !== null);
  protected readonly predicted = computed(() => this.entry().predHomeGoals !== null);
}
