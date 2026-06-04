export type Role = 'Admin' | 'User';

export interface RegisterRequest {
  email: string;
  password: string;
  displayName: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  expiresAtUtc: string;
  role: Role;
  displayName: string;
}

/** Authenticated user as held in client state. */
export interface CurrentUser {
  token: string;
  role: Role;
  displayName: string;
  expiresAtUtc: string;
  userId: string;
}
