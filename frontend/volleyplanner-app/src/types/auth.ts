export interface RegisterRequest {
  name: string;
  email: string;
  password: string;
}

export interface RegisterResponse {
  userId: number;
  name: string;
  email: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  name: string;
  email: string;
}

export interface UserProfileResponse {
  userId: number;
  name: string;
  email: string;
  level?: string | null;
  goal?: string | null;
  age?: number | null;
  height?: number | null;
  weight?: number | null;
}