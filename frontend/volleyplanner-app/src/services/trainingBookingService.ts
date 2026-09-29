import api from "../api/axios";
import type { TrainingBooking } from "../types/trainingBooking";

export const trainingBookingService = {
  async getMine(): Promise<TrainingBooking[]> {
    const response =
      await api.get<TrainingBooking[]>(
        "/TrainingBookings/mine"
      );

    return response.data;
  },
};