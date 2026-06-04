import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LeaderboardEntry, UserHistory } from '../models/leaderboard.models';

@Injectable({ providedIn: 'root' })
export class LeaderboardService {
  private readonly http = inject(HttpClient);
  private readonly base = environment.apiBaseUrl;

  getLeaderboard(): Observable<LeaderboardEntry[]> {
    return this.http.get<LeaderboardEntry[]>(`${this.base}/leaderboard`);
  }

  getUserHistory(userId: string): Observable<UserHistory> {
    return this.http.get<UserHistory>(`${this.base}/users/${userId}/predictions`);
  }
}
