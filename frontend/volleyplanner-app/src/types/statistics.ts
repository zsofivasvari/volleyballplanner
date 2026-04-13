export interface PlanUsageStat {
  trainingPlanId: number;
  title: string;
  count: number;
}

export interface DailyTrainingStat {
  dayLabel: string;
  count: number;
  durationMinutes: number;
}

export interface FocusUsageStat {
  focusName: string;
  count: number;
}

export interface StatisticsSummary {
  weeklyCount: number;
  monthlyCount: number;
  totalEvents: number;
  totalDurationMinutes: number;
  topPlans: PlanUsageStat[];
  weeklyDailyStats: DailyTrainingStat[];
  focusUsageStats: FocusUsageStat[];
}