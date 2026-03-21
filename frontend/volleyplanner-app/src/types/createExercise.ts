export interface CreateExerciseRequest {
  title: string;
  description: string;
  sportType: string;
  durationMin: number;
  difficulty: string;
  intensity: string;
  minPlayers: number;
  maxPlayers: number;
  phase: string;
  tagIds: number[];
}