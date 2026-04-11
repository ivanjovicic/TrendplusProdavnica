"use client";

import { useEffect, useState } from "react";
import { adminApiClient, AdminListResponse, OrderListItem } from "@/lib/admin/client";
import { useAdmin } from "@/lib/admin/context";
import Link from "next/link";

const ORDER_STATUSES = ["pending", "confirmed", "shipped", "delivered", "cancelled"];

export default function AdminOrdersPage() {
  const { isAuthenticated } = useAdmin();
  const [orders, setOrders] = useState<AdminListResponse<OrderListItem> | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [statusFilter, setStatusFilter] = useState<string>("");
  const [pageSize] = useState(20);

  useEffect(() => {
    if (!isAuthenticated) return;

    const loadOrders = async () => {
      try {
        setIsLoading(true);
        const result = await adminApiClient.getOrders(
          1,
          pageSize,
          statusFilter || undefined
        );
        setOrders(result);
      } catch (error) {
        console.error("Failed to load orders:", error);
      } finally {
        setIsLoading(false);
      }
    };

    loadOrders();
  }, [isAuthenticated, pageSize, statusFilter]);

  const getStatusColor = (status: string) => {
    switch (status) {
      case "pending":
        return "bg-yellow-50 text-yellow-700";
      case "confirmed":
        return "bg-blue-50 text-blue-700";
      case "shipped":
        return "bg-purple-50 text-purple-700";
      case "delivered":
        return "bg-green-50 text-green-700";
      case "cancelled":
        return "bg-red-50 text-red-700";
      default:
        return "bg-slate-50 text-slate-700";
    }
  };

  if (isLoading) {
    return <div className="text-slate-600">Loading orders...</div>;
  }

  return (
    <div className="space-y-6">
      <h1 className="text-3xl font-light">Orders</h1>

      {/* Filters */}
      <div>
        <select
          value={statusFilter}
          onChange={(e) => {
            setStatusFilter(e.target.value);
          }}
          className="px-4 py-2 border border-slate-300 rounded focus:outline-none focus:ring-2 focus:ring-slate-900"
        >
          <option value="">All Statuses</option>
          {ORDER_STATUSES.map((status) => (
            <option key={status} value={status}>
              {status.charAt(0).toUpperCase() + status.slice(1)}
            </option>
          ))}
        </select>
      </div>

      {/* Orders Table */}
      <div className="bg-white border border-slate-200 rounded overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead className="bg-slate-50 border-b border-slate-200">
              <tr>
                <th className="px-6 py-4 text-left font-medium text-slate-700">
                  Order #
                </th>
                <th className="px-6 py-4 text-left font-medium text-slate-700">
                  Customer
                </th>
                <th className="px-6 py-4 text-left font-medium text-slate-700">
                  Items
                </th>
                <th className="px-6 py-4 text-left font-medium text-slate-700">
                  Total
                </th>
                <th className="px-6 py-4 text-left font-medium text-slate-700">
                  Status
                </th>
                <th className="px-6 py-4 text-left font-medium text-slate-700">
                  Date
                </th>
                <th className="px-6 py-4 text-left font-medium text-slate-700">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody>
              {orders?.items.map((order) => (
                <tr
                  key={order.id}
                  className="border-b border-slate-100 hover:bg-slate-50"
                >
                  <td className="px-6 py-4 font-mono text-xs font-medium">
                    {order.orderNumber}
                  </td>
                  <td className="px-6 py-4">
                    <div>
                      <p className="font-medium">{order.customerName}</p>
                      <p className="text-slate-600 text-xs">{order.customerEmail}</p>
                    </div>
                  </td>
                  <td className="px-6 py-4">
                    <span className="inline-block px-2 py-1 bg-slate-100 text-slate-700 rounded text-xs">
                      {order.itemCount} items
                    </span>
                  </td>
                  <td className="px-6 py-4 font-medium">
                    ${order.totalAmount.toFixed(2)}
                  </td>
                  <td className="px-6 py-4">
                    <span
                      className={`inline-block px-2 py-1 rounded text-xs font-medium ${getStatusColor(
                        order.status
                      )}`}
                    >
                      {order.status.charAt(0).toUpperCase() + order.status.slice(1)}
                    </span>
                  </td>
                  <td className="px-6 py-4 text-slate-600">
                    {new Date(order.createdUtc).toLocaleDateString()}
                  </td>
                  <td className="px-6 py-4">
                    <Link
                      href={`/admin/orders/${order.id}`}
                      className="text-slate-600 hover:text-slate-900"
                    >
                      View
                    </Link>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        {/* Pagination */}
        <div className="px-6 py-4 border-t border-slate-200 flex items-center justify-between">
          <div className="text-sm text-slate-600">
            {orders?.totalCount || 0} total items
          </div>
          <div className="space-x-2">
            {/* Pagination buttons disabled for now */}
          </div>
        </div>
      </div>
    </div>
  );
}
