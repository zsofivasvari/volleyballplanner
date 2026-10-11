import api from "../api/axios";

import type {
  CreateTeamInvitationRequest,
  TeamInvitation,
  TournamentEntryResponse,
} from "../types/tournament";

export const teamInvitationService = {
  async create(
    request: CreateTeamInvitationRequest
  ): Promise<TeamInvitation> {
    const response =
      await api.post<TeamInvitation>(
        "/TeamInvitations",
        request
      );

    return response.data;
  },

  async getReceived(): Promise<
    TeamInvitation[]
  > {
    const response =
      await api.get<TeamInvitation[]>(
        "/TeamInvitations/received"
      );

    return response.data;
  },

  async getSent(): Promise<
    TeamInvitation[]
  > {
    const response =
      await api.get<TeamInvitation[]>(
        "/TeamInvitations/sent"
      );

    return response.data;
  },

  async accept(
    invitationId: number
  ): Promise<TournamentEntryResponse> {
    const response =
      await api.post<TournamentEntryResponse>(
        `/TeamInvitations/${invitationId}/accept`
      );

    return response.data;
  },

  async reject(
    invitationId: number
  ): Promise<TeamInvitation> {
    const response =
      await api.post<TeamInvitation>(
        `/TeamInvitations/${invitationId}/reject`
      );

    return response.data;
  },
};