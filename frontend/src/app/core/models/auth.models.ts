export type Role = 'Admin' | 'User';

export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  tokenType: string;
  expiresAt: string;
  username: string;
  role: Role;
}

/** Sesión guardada en el navegador. */
export interface Session {
  token: string;
  username: string;
  role: Role;
  expiresAt: string;
}
