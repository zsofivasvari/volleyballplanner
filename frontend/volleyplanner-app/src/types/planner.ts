export interface CalendarEvent {
  id: number;
  trainingPlanId: number;
  title: string;
  sportType: string;
  startTime: string;
  endTime: string;
  status: string;
}

export interface CreateCalendarEventRequest {
  trainingPlanId: number;
  title: string;
  startTime: string;
  endTime: string;
}