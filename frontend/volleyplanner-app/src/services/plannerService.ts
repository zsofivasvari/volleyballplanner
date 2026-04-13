import api from "../api/axios";
import type { CalendarEvent, CreateCalendarEventRequest } from "../types/planner";

export const plannerService = {
  async getAll(): Promise<CalendarEvent[]> {
    const response = await api.get<CalendarEvent[]>("/CalendarEvents");
    return response.data;
  },

  async create(data: CreateCalendarEventRequest): Promise<CalendarEvent> {
    const response = await api.post<CalendarEvent>("/CalendarEvents", data);
    return response.data;
  },

  async remove(id: number): Promise<void> {
    await api.delete(`/CalendarEvents/${id}`);
  },
};