export type MatchStatus = 'Scheduled' | 'Finished';

export interface TeamLite {
  code: string;
  name: string;
  groupName: string;
}

export interface MatchResult {
  homeGoals: number;
  awayGoals: number;
}

export interface MyPrediction {
  matchId: number;
  homeGoals: number;
  awayGoals: number;
  pointsAwarded: number | null;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface MatchVm {
  id: number;
  group: string;
  homeTeam: TeamLite;
  awayTeam: TeamLite;
  kickoffUtc: string;
  status: MatchStatus;
  isLocked: boolean;
  result: MatchResult | null;
  myPrediction: MyPrediction | null;
}

export interface UpsertPredictionRequest {
  homeGoals: number;
  awayGoals: number;
}
