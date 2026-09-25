import api from "../api/axios";
import type {
  SportRolesResponse,
  UpdateSportRolesRequest,
} from "../types/sportRole";

export const sportRoleService = {
  async getMySportRoles(): Promise<SportRolesResponse> {
    const response = await api.get<SportRolesResponse>(
      "/users/me/sport-roles"
    );

    return response.data;
  },

  async updateMySportRoles(
    data: UpdateSportRolesRequest
  ): Promise<SportRolesResponse> {
    const response = await api.put<SportRolesResponse>(
      "/users/me/sport-roles",
      data
    );

    return response.data;
  },
};