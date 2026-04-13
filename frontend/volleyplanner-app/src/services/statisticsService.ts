import api from "../api/axios";
import type { StatisticsSummary } from "../types/statistics";

export const statisticsService = {
  async getMyStatistics(range: string = "30days"): Promise<StatisticsSummary> {
    const response = await api.get<StatisticsSummary>(
      `/Statistics?range=${encodeURIComponent(range)}`
    );
    return response.data;
  },
};