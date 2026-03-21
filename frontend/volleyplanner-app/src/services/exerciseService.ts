import api from "../api/axios";
import type {
  ExerciseDetails,
  ExerciseListItem,
  ExerciseQuery,
} from "../types/exercise";
import type { CreateExerciseRequest } from "../types/createExercise";

export const exerciseService = {
  async getAll(filters?: ExerciseQuery): Promise<ExerciseListItem[]> {
    const response = await api.get<ExerciseListItem[]>("/Exercises", {
      params: filters,
    });
    return response.data;
  },

  async getById(id: number): Promise<ExerciseDetails> {
    const response = await api.get<ExerciseDetails>(`/Exercises/${id}`);
    return response.data;
  },

  async create(data: CreateExerciseRequest): Promise<ExerciseDetails> {
    const response = await api.post<ExerciseDetails>("/Exercises", data);
    return response.data;
  },
};