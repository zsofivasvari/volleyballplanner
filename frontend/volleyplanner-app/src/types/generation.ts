export interface GenerateTrainingPlanRequest {
  sportTypes: string[];
  durationMin: number;
  playerCount?: number | null;
  intensities: string[];
  difficulties: string[];
  focusAreas: string[];
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
  focusAreas: string[];
  items: GeneratedTrainingPlanItem[];
}