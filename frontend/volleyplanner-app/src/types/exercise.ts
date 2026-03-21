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
}

export interface ExerciseQuery {
  sportType?: string;
  difficulty?: string;
  intensity?: string;
  phase?: string;
}