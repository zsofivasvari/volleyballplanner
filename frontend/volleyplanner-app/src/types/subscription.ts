export interface SubscriptionStatus {
  plan: "Free" | "Premium" | "Admin";
  isPremium: boolean;
  isAdmin: boolean;
  freeGenerationsUsed: number;
  freeGenerationLimit: number;
  remainingFreeGenerations: number;
  subscriptionStatus: string | null;
  currentPeriodEnd: string | null;
  cancelAtPeriodEnd: boolean;
}

export interface CreateCheckoutSessionResponse {
  checkoutUrl: string;
}