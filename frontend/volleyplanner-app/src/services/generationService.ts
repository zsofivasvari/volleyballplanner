import api from "../api/axios";
import type {
  GenerateTrainingPlanRequest,
  GeneratedTrainingPlanResponse,
} from "../types/generation";

export const generationService = {
  async generate(
    data: GenerateTrainingPlanRequest
  ): Promise<GeneratedTrainingPlanResponse> {
    const token = localStorage.getItem("token");

    const response = await api.post<GeneratedTrainingPlanResponse>(
      "/Generation",
      data,
      {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      }
    );

    return response.data;
  },
};