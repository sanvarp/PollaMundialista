import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { MatchVm, MyPrediction, UpsertPredictionRequest } from '../models/match.models';

@Injectable({ providedIn: 'root' })
export class MatchesService {
  private readonly http = inject(HttpClient);
  private readonly base = environment.apiBaseUrl;

  getMatches(): Observable<MatchVm[]> {
    return this.http.get<MatchVm[]>(`${this.base}/matches`);
  }

  upsertPrediction(matchId: number, body: UpsertPredictionRequest): Observable<MyPrediction> {
    return this.http.put<MyPrediction>(`${this.base}/predictions/${matchId}`, body);
  }
}
