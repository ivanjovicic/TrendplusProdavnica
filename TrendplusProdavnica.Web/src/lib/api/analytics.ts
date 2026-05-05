import { getApiClient } from './index';

export interface ShoeTypeSalesStats {
  categoryId: number | null;
  categoryName: string | null;
  totalOrders: number;
  completedOrders: number;
  pendingOrders: number;
  totalRevenue: number;
  averageOrderValue: number;
  unitsOrdered: number;
  averageUnitsPerOrder: number;
  conversionRate: number;
  productViews: number;
  periodStart: string;
  periodEnd: string;
  calculatedAtUtc: string;
  dataVersion: string;
  isAggregated: boolean;
  sourceRecordCount: number;
}

export interface ShoeTypeSalesReport {
  shoeTypes: ShoeTypeSalesStats[];
  reportGeneratedAtUtc: string;
  periodStart: string;
  periodEnd: string;
  totalTypesIncluded: number;
  totalMarketRevenue: number;
  totalMarketOrders: number;
}

export interface SupplierSalesStats {
  brandId: number;
  brandName: string;
  totalOrders: number;
  completedOrders: number;
  pendingOrders: number;
  totalRevenue: number;
  averageOrderValue: number;
  unitsOrdered: number;
  averageUnitsPerOrder: number;
  conversionRate: number;
  productViews: number;
  periodStart: string;
  periodEnd: string;
  calculatedAtUtc: string;
  dataVersion: string;
  isAggregated: boolean;
  sourceRecordCount: number;
}

export interface SupplierSalesReport {
  suppliers: SupplierSalesStats[];
  reportGeneratedAtUtc: string;
  periodStart: string;
  periodEnd: string;
  totalSuppliersIncluded: number;
  totalMarketRevenue: number;
  totalMarketOrders: number;
}

export interface AnalyticsQueryParams {
  from?: string;
  to?: string;
  limit?: number;
  brandId?: number;
  categoryId?: number;
  includeSubcategories?: boolean;
}

export const analyticsApi = {
  getShoeTypeSalesStats: (params?: AnalyticsQueryParams) =>
    getApiClient.get<ShoeTypeSalesReport>('/analytics/shoe-type-sales-stats', { searchParams: params }),

  getSupplierSalesStats: (params?: AnalyticsQueryParams) =>
    getApiClient.get<SupplierSalesReport>('/analytics/supplier-sales-stats', { searchParams: params }),
};
