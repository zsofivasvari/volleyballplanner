import api from "../api/axios";
import type { Tag } from "../types/tag";

export const tagService = {
  async getAll(): Promise<Tag[]> {
    const response = await api.get<Tag[]>("/Tags");
    return response.data;
  },
};