import api from "../api/axios";
import type {
  ExerciseDetails,
  ExerciseQuery,
  PagedExercisesResponse,
} from "../types/exercise";
import type { CreateExerciseRequest } from "../types/createExercise";

export const exerciseService = {
  async getAll(
    filters?: ExerciseQuery & { page?: number; pageSize?: number }
  ): Promise<PagedExercisesResponse> {
    const params = new URLSearchParams();

    filters?.sportTypes?.forEach((value) => {
      params.append("SportTypes", value);
    });

    filters?.difficulties?.forEach((value) => {
      params.append("Difficulties", value);
    });

    filters?.intensities?.forEach((value) => {
      params.append("Intensities", value);
    });

    filters?.phases?.forEach((value) => {
      params.append("Phases", value);
    });

    filters?.focusTags?.forEach((value) => {
      params.append("FocusTags", value);
    });

    if (filters?.page) {
      params.append("Page", filters.page.toString());
    }

    if (filters?.pageSize) {
      params.append("PageSize", filters.pageSize.toString());
    }

    const response = await api.get<PagedExercisesResponse>(
      `/Exercises?${params.toString()}`
    );

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