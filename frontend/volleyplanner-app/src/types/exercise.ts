export interface ExerciseListItem {
  id: number;
  title: string;
  sportType: string;
  durationMin: number;
  difficulty: string;
  intensity: string;
  minPlayers: number;
  maxPlayers: number;
  phase: string;
}

export interface ExerciseDetails {
  id: number;
  title: string;
  description: string;
  sportType: string;
  durationMin: number;
  difficulty: string;
  intensity: string;
  minPlayers: number;
  maxPlayers: number;
  phase: string;
  tags: string[];
  focusTags: string[];
}

export interface ExerciseQuery {
  sportTypes: string[];
  difficulties: string[];
  intensities: string[];
  phases: string[];
}

export interface PagedExercisesResponse {
  items: ExerciseListItem[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}