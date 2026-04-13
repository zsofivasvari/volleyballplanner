import api from "../api/axios";
import type { Tag } from "../types/tag";

export const tagService = {
  async getAll(type?: string): Promise<Tag[]> {
    const query = type ? `?type=${encodeURIComponent(type)}` : "";
    const response = await api.get<Tag[]>(`/Tags${query}`);
    return response.data;
  },
};