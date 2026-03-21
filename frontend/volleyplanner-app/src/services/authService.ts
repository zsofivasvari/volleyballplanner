import api from "../api/axios";
import type {
  LoginRequest,
  LoginResponse,
  RegisterRequest,
  RegisterResponse,
  UserProfileResponse,
} from "../types/auth";

export const authService = {
  async register(data: RegisterRequest): Promise<RegisterResponse> {
    const response = await api.post<RegisterResponse>("/Auth/register", data);
    return response.data;
  },

  async login(data: LoginRequest): Promise<LoginResponse> {
    const response = await api.post<LoginResponse>("/Auth/login", data);
    return response.data;
  },

  async getMe(): Promise<UserProfileResponse> {
    const token = localStorage.getItem("token");

    const response = await api.get<UserProfileResponse>("/Auth/me", {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });

    return response.data;
  },
};