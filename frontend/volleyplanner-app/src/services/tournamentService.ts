import api from "../api/axios";

import type {
  TournamentBracket,
  TournamentDetails,
  TournamentEntryListItem,
  TournamentFinalResult,
  TournamentListItem,
} from "../types/tournament";

export const tournamentService = {
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
};