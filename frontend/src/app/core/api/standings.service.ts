import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { GroupStanding } from '../models/standings.models';

@Injectable({ providedIn: 'root' })
export class StandingsService {
  private readonly http = inject(HttpClient);
  private readonly base = environment.apiBaseUrl;

  getStandings(): Observable<GroupStanding[]> {
    return this.http.get<GroupStanding[]>(`${this.base}/standings`);
  }
}
