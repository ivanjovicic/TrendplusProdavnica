"use client";

import { useEffect, useState } from "react";
import { adminApiClient, ProductListItem, OrderListItem } from "@/lib/admin/client";
import { useAdmin } from "@/lib/admin/context";

export default function AdminDashboard() {
  const { isAuthenticated } = useAdmin();
  const [stats, setStats] = useState({
    recentProducts: [] as ProductListItem[],
    recentOrders: [] as OrderListItem[],
    productCount: 0,
    orderCount: 0,
  });
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    if (!isAuthenticated) return;

    const loadData = async () => {
      try {
        setIsLoading(true);
        const [productsRes, ordersRes] = await Promise.all([
          adminApiClient.getProducts(1, 5),
          adminApiClient.getOrders(1, 5),
        ]);

        setStats({
          recentProducts: productsRes.items,
          recentOrders: ordersRes.items,
          productCount: productsRes.totalCount,
          orderCount: ordersRes.totalCount,
        });
      } catch (error) {
        console.error("Failed to load dashboard data:", error);
      } finally {
        setIsLoading(false);
      }
    };

    loadData();
  }, [isAuthenticated]);

  if (isLoading) {
    return <div className="text-slate-600">Loading dashboard...</div>;
  }

  return (
    <div className="space-y-8">
      {/* Stats Grid */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
        <div className="bg-white border border-slate-200 rounded p-6">
          <p className="text-slate-600 text-sm font-medium mb-2">Total Products</p>
          <p className="text-3xl font-light">{stats.productCount}</p>
        </div>
        <div className="bg-white border border-slate-200 rounded p-6">
          <p className="text-slate-600 text-sm font-medium mb-2">Total Orders</p>
          <p className="text-3xl font-light">{stats.orderCount}</p>
        </div>
        <div className="bg-white border border-slate-200 rounded p-6">
          <p className="text-slate-600 text-sm font-medium mb-2">Pending Orders</p>
          <p className="text-3xl font-light">
            {stats.recentOrders.filter((o) => o.status === "pending").length}
          </p>
        </div>
        <div className="bg-white border border-slate-200 rounded p-6">
          <p className="text-slate-600 text-sm font-medium mb-2">Active Products</p>
          <p className="text-3xl font-light">
            {stats.recentProducts.filter((p) => p.isActive).length}
          </p>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
        {/* Recent Products */}
        <div className="bg-white border border-slate-200 rounded">
          <div className="p-6 border-b border-slate-200">
            <h3 className="text-lg font-light">Recent Products</h3>
          </div>
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead className="bg-slate-50">
                <tr>
                  <th className="px-6 py-3 text-left font-medium text-slate-700">
                    Name
                  </th>
                  <th className="px-6 py-3 text-left font-medium text-slate-700">
                    Price
                  </th>
                  <th className="px-6 py-3 text-left font-medium text-slate-700">
                    Stock
                  </th>
                </tr>
              </thead>
              <tbody>
                {stats.recentProducts.map((product) => (
                  <tr
                    key={product.id}
                    className="border-b border-slate-100 hover:bg-slate-50"
                  >
                    <td className="px-6 py-4">{product.name}</td>
                    <td className="px-6 py-4">${product.price.toFixed(2)}</td>
                    <td className="px-6 py-4">{product.stockQuantity}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>

        {/* Recent Orders */}
        <div className="bg-white border border-slate-200 rounded">
          <div className="p-6 border-b border-slate-200">
            <h3 className="text-lg font-light">Recent Orders</h3>
          </div>
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead className="bg-slate-50">
                <tr>
                  <th className="px-6 py-3 text-left font-medium text-slate-700">
                    Order #
                  </th>
                  <th className="px-6 py-3 text-left font-medium text-slate-700">
                    Customer
                  </th>
                  <th className="px-6 py-3 text-left font-medium text-slate-700">
                    Status
                  </th>
                </tr>
              </thead>
              <tbody>
                {stats.recentOrders.map((order) => (
                  <tr
                    key={order.id}
                    className="border-b border-slate-100 hover:bg-slate-50"
                  >
                    <td className="px-6 py-4 font-mono text-xs">
                      {order.orderNumber}
                    </td>
                    <td className="px-6 py-4">{order.customerName}</td>
                    <td className="px-6 py-4">
                      <span className="inline-block px-2 py-1 bg-slate-100 text-slate-700 rounded text-xs">
                        {order.status}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  );
}
