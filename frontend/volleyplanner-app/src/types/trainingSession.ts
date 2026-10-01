export interface TrainingSessionListItem {
  id: number;
  title: string;
  sportType: string;

  startTime: string;
  endTime: string;

  location: string;

  maxParticipants: number;
  participantCount: number;
  waitlistCount: number;
  isFull: boolean;

  targetLevel: string;
  status: string;

  organizerUserId: number;
  organizerName: string;
}

export interface TrainingSessionDetails {
  id: number;

  organizerUserId: number;
  organizerName: string;

  trainingPlanId?: number | null;
  trainingPlanTitle?: string | null;

  title: string;
  description: string;
  sportType: string;

  startTime: string;
  endTime: string;

  location: string;

  maxParticipants: number;
  participantCount: number;
  waitlistCount: number;
  isFull: boolean;

  targetLevel: string;
  status: string;

  createdAt: string;

  currentUserBookingStatus?: string | null;
  waitlistPosition?: number | null;

  isInCurrentUserCalendar: boolean;
}

export interface CreateTrainingSessionRequest {
  trainingPlanId?: number | null;

  title: string;
  description: string;
  sportType: string;

  startTime: string;
  endTime: string;

  location: string;

  maxParticipants: number;

  targetLevel: string;
}

export interface UpdateTrainingSessionRequest {
  title: string;
  description: string;
  sportType: string;

  startTime: string;
  endTime: string;

  location: string;

  maxParticipants: number;

  targetLevel: string;
}

export interface AssignTrainingPlanRequest {
  trainingPlanId: number;
}