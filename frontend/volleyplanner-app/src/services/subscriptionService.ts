import api from "../api/axios";
import type {
  CreateCheckoutSessionResponse,
  SubscriptionStatus,
} from "../types/subscription";

export const subscriptionService = {
  async getMyStatus(): Promise<SubscriptionStatus> {
    const response =
      await api.get<SubscriptionStatus>(
        "/Subscriptions/me"
      );

    return response.data;
  },

  async createCheckoutSession():
    Promise<CreateCheckoutSessionResponse> {
    const response =
      await api.post<CreateCheckoutSessionResponse>(
        "/Subscriptions/checkout"
      );

    return response.data;
  },

  async syncSubscription(
    sessionId?: string
  ): Promise<SubscriptionStatus> {
    const response =
      await api.post<SubscriptionStatus>(
        "/Subscriptions/sync",
        null,
        {
          params: sessionId
            ? { sessionId }
            : undefined,
        }
      );

    return response.data;
  },

  async cancelSubscription(): Promise<{ message: string }> {
    const response =
      await api.post<{ message: string }>(
        "/Subscriptions/cancel"
      );

    return response.data;
  },

  async reactivateSubscription(): Promise<{ message: string }> {
    const response =
      await api.post<{ message: string }>(
        "/Subscriptions/reactivate"
      );

    return response.data;
  },

  async createPortalSession(): Promise<{ portalUrl: string }> {
    const response =
      await api.post<{ portalUrl: string }>(
        "/Subscriptions/portal"
      );

    return response.data;
  },
  };