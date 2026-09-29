export interface TrainingBooking {
  id: number;

  trainingSessionId: number;
  trainingSessionTitle: string;

  userId: number;
  userName: string;

  status: string;

  createdAt: string;

  waitlistPosition?: number | null;
}

export interface TrainingParticipant {
  userId: number;

  userName: string;

  status: string;

  registeredAt: string;

  waitlistPosition?: number | null;
}