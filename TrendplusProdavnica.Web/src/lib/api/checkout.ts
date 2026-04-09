import { apiClient } from './api-client';
import type {
  CheckoutSummaryDto,
  CheckoutRequest,
  CheckoutResultDto,
  OrderDto,
} from '@/lib/types';

export async function getCheckoutSummary(cartToken: string) {
  return apiClient.get<CheckoutSummaryDto>(`/checkout/${cartToken}`);
}

export async function placeOrder(payload: CheckoutRequest) {
  return apiClient.post<CheckoutResultDto>('/checkout', payload);
}

export async function getOrder(orderNumber: string) {
  return apiClient.get<OrderDto>(`/orders/${orderNumber}`);
}
