export interface GenerateTrainingPlanRequest {
  sportTypes: string[];
  durationMin: number;
  playerCount: number;
  intensities: string[];
  difficulties: string[];
  primaryFocus: string;
}

export interface GeneratedTrainingPlanItem {
  exerciseId: number;
  exerciseTitle: string;
  sectionName: string;
  plannedDuration: number;
}

export interface GeneratedTrainingPlanResponse {
  sportType: string;
  targetDuration: number;
  difficulty: string;
  intensity: string;
  primaryFocus: string;
  items: GeneratedTrainingPlanItem[];
}