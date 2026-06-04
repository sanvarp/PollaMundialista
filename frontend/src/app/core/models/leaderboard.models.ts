import { MatchResult, TeamLite } from './match.models';

export interface LeaderboardEntry {
  position: number;
  userId: string;
  displayName: string;
  totalPoints: number;
  exactHits: number;
}

export interface UserHistoryEntry {
  matchId: number;
  group: string;
  homeTeam: TeamLite;
  awayTeam: TeamLite;
  kickoffUtc: string;
  status: 'Scheduled' | 'Finished';
  result: MatchResult | null;
  predHomeGoals: number;
  predAwayGoals: number;
  pointsAwarded: number | null;
}

export interface UserHistory {
  userId: string;
  displayName: string;
  totalPoints: number;
  exactHits: number;
  isOwnerView: boolean;
  predictions: UserHistoryEntry[];
}
