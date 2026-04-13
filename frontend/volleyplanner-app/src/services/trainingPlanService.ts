import api from "../api/axios";
import type {
  CreateTrainingPlanRequest,
  TrainingPlanDetails,
  TrainingPlanListItem,
} from "../types/trainingPlan";

export const trainingPlanService = {
  async getAll(): Promise<TrainingPlanListItem[]> {
    const token = localStorage.getItem("token");

    const response = await api.get<TrainingPlanListItem[]>("/TrainingPlans", {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });

    return response.data;
  },

  async getById(id: number): Promise<TrainingPlanDetails> {
    const token = localStorage.getItem("token");

    const response = await api.get<TrainingPlanDetails>(`/TrainingPlans/${id}`, {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });

    return response.data;
  },

  async create(data: CreateTrainingPlanRequest): Promise<TrainingPlanDetails> {
    const token = localStorage.getItem("token");

    const response = await api.post<TrainingPlanDetails>("/TrainingPlans", data, {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });

    return response.data;
  },
};