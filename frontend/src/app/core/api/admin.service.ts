import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { MatchVm } from '../models/match.models';

export interface SetResultRequest {
  homeGoals: number;
  awayGoals: number;
}

@Injectable({ providedIn: 'root' })
export class AdminService {
  private readonly http = inject(HttpClient);
  private readonly base = environment.apiBaseUrl;

  setResult(matchId: number, body: SetResultRequest): Observable<MatchVm> {
    return this.http.put<MatchVm>(`${this.base}/admin/matches/${matchId}/result`, body);
  }
}
