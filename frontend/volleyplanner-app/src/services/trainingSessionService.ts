import api from "../api/axios";

import type {
  AssignTrainingPlanRequest,
  CreateTrainingSessionRequest,
  UpdateTrainingSessionRequest,
  TrainingSessionDetails,
  TrainingSessionListItem,
} from "../types/trainingSession";

import type {
  TrainingBooking,
  TrainingParticipant,
} from "../types/trainingBooking";

export const trainingSessionService = {
  async getAll(): Promise<TrainingSessionListItem[]> {
    const response =
      await api.get<TrainingSessionListItem[]>(
        "/TrainingSessions"
      );

    return response.data;
  },

  async getMine(): Promise<TrainingSessionListItem[]> {
    const response =
      await api.get<TrainingSessionListItem[]>(
        "/TrainingSessions/mine"
      );

    return response.data;
  },

  async getById(
    id: number
  ): Promise<TrainingSessionDetails> {
    const response =
      await api.get<TrainingSessionDetails>(
        `/TrainingSessions/${id}`
      );

    return response.data;
  },

  async create(
    data: CreateTrainingSessionRequest
  ): Promise<TrainingSessionDetails> {
    const response =
      await api.post<TrainingSessionDetails>(
        "/TrainingSessions",
        data
      );

    return response.data;
  },

  async update(
    id: number,
    data: UpdateTrainingSessionRequest
  ): Promise<TrainingSessionDetails> {
    const response =
      await api.put<TrainingSessionDetails>(
        `/TrainingSessions/${id}`,
        data
      );

    return response.data;
  },

  async delete(
    id: number
  ): Promise<void> {
    await api.delete(
      `/TrainingSessions/${id}`
    );
  },

  async assignTrainingPlan(
    id: number,
    trainingPlanId: number
  ): Promise<TrainingSessionDetails> {
    const data: AssignTrainingPlanRequest = {
      trainingPlanId,
    };

    const response =
      await api.put<TrainingSessionDetails>(
        `/TrainingSessions/${id}/training-plan`,
        data
      );

    return response.data;
  },

  async removeTrainingPlan(
    id: number
  ): Promise<void> {
    await api.delete(
      `/TrainingSessions/${id}/training-plan`
    );
  },

  async book(
    id: number
  ): Promise<TrainingBooking> {
    const response =
      await api.post<TrainingBooking>(
        `/TrainingSessions/${id}/book`
      );

    return response.data;
  },

  async cancelBooking(
    id: number
  ): Promise<void> {
    await api.delete(
      `/TrainingSessions/${id}/book`
    );
  },

  async getParticipants(
    id: number
  ): Promise<TrainingParticipant[]> {
    const response =
      await api.get<TrainingParticipant[]>(
        `/TrainingSessions/${id}/participants`
      );

    return response.data;
  },
};