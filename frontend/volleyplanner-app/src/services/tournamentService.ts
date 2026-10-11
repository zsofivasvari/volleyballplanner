import api from "../api/axios";

import type {
  CreateTournamentRequest,
  TournamentBracket,
  TournamentDetails,
  TournamentEntryListItem,
  TournamentFinalResult,
  TournamentListItem,
  TournamentMatchResultResponse,
  SubmitTournamentMatchResultRequest,
  TournamentDrawResponse,
} from "../types/tournament";

export const tournamentService = {
  async create(
    request: CreateTournamentRequest
  ): Promise<TournamentDetails> {
    const response =
      await api.post<TournamentDetails>(
        "/Tournaments",
        request
      );

    return response.data;
  },

  async getAll(): Promise<TournamentListItem[]> {
    const response =
      await api.get<TournamentListItem[]>(
        "/Tournaments"
      );

    return response.data;
  },

  async getMine(): Promise<TournamentListItem[]> {
    const response =
      await api.get<TournamentListItem[]>(
        "/Tournaments/mine"
      );

    return response.data;
  },

  async getById(
    id: number
  ): Promise<TournamentDetails> {
    const response =
      await api.get<TournamentDetails>(
        `/Tournaments/${id}`
      );

    return response.data;
  },

  async getEntries(
    id: number
  ): Promise<TournamentEntryListItem[]> {
    const response =
      await api.get<TournamentEntryListItem[]>(
        `/Tournaments/${id}/entries`
      );

    return response.data;
  },

  async getBracket(
    id: number
  ): Promise<TournamentBracket> {
    const response =
      await api.get<TournamentBracket>(
        `/Tournaments/${id}/bracket`
      );

    return response.data;
  },

  async getFinalResult(
    id: number
  ): Promise<TournamentFinalResult> {
    const response =
      await api.get<TournamentFinalResult>(
        `/Tournaments/${id}/final-result`
      );

    return response.data;
  },

  async submitMatchResult(
    tournamentId: number,
    matchId: number,
    request: SubmitTournamentMatchResultRequest
  ): Promise<TournamentMatchResultResponse> {
    const response =
      await api.post<TournamentMatchResultResponse>(
        `/Tournaments/${tournamentId}/matches/${matchId}/result`,
        request
      );

    return response.data;
  },

  async drawTournament(
    tournamentId: number
  ): Promise<TournamentDrawResponse> {
    const response =
      await api.post<TournamentDrawResponse>(
        `/Tournaments/${tournamentId}/draw`
      );

    return response.data;
  },
};