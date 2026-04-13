export interface CreateTrainingPlanItem {
  exerciseId: number;
  orderIndex: number;
  sectionName: string;
  plannedDuration: number;
}

export interface CreateTrainingPlanRequest {
  title: string;
  sportType: string;
  planType: string;
  targetDuration: number;
  targetIntensity: string;
  targetLevel: string;
  primaryFocus: string;
  items: CreateTrainingPlanItem[];
}

export interface TrainingPlanListItem {
  id: number;
  title: string;
  sportType: string;
  targetDuration: number;
  primaryFocus: string;
  createdAt: string;
}

export interface TrainingPlanDetailsItem {
  exerciseId: number;
  exerciseTitle: string;
  orderIndex: number;
  sectionName: string;
  plannedDuration: number;
}

export interface TrainingPlanDetails {
  id: number;
  title: string;
  sportType: string;
  planType: string;
  targetDuration: number;
  targetIntensity: string;
  targetLevel: string;
  primaryFocus: string;
  createdAt: string;
  items: TrainingPlanDetailsItem[];
}