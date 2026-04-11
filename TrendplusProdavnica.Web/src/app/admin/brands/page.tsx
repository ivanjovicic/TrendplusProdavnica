"use client";

import { useEffect, useState } from "react";
import { adminApiClient, AdminListResponse, BrandListItem } from "@/lib/admin/client";
import { useAdmin } from "@/lib/admin/context";

export default function AdminBrandsPage() {
  const { isAuthenticated } = useAdmin();
  const [brands, setBrands] = useState<AdminListResponse<BrandListItem> | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [pageSize] = useState(20);

  useEffect(() => {
    if (!isAuthenticated) return;

    const loadBrands = async () => {
      try {
        setIsLoading(true);
        const result = await adminApiClient.getBrands(1, pageSize);
        setBrands(result);
      } catch (error) {
        console.error("Failed to load brands:", error);
      } finally {
        setIsLoading(false);
      }
    };

    loadBrands();
  }, [isAuthenticated, pageSize]);

  if (isLoading) {
    return <div className="text-slate-600">Loading brands...</div>;
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-light">Brands</h1>
        <button className="px-4 py-2 bg-slate-900 text-white rounded hover:bg-slate-800 transition">
          + New Brand
        </button>
      </div>

      <div className="bg-white border border-slate-200 rounded overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead className="bg-slate-50 border-b border-slate-200">
              <tr>
                <th className="px-6 py-4 text-left font-medium text-slate-700">Name</th>
                <th className="px-6 py-4 text-left font-medium text-slate-700">
                  Products
                </th>
                <th className="px-6 py-4 text-left font-medium text-slate-700">
                  Status
                </th>
                <th className="px-6 py-4 text-left font-medium text-slate-700">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody>
              {brands?.items.map((brand) => (
                <tr key={brand.id} className="border-b border-slate-100 hover:bg-slate-50">
                  <td className="px-6 py-4 font-medium">{brand.name}</td>
                  <td className="px-6 py-4 text-slate-600">{brand.productCount}</td>
                  <td className="px-6 py-4">
                    <span
                      className={`inline-block px-2 py-1 rounded text-xs font-medium ${
                        brand.isActive
                          ? "bg-green-50 text-green-700"
                          : "bg-slate-100 text-slate-700"
                      }`}
                    >
                      {brand.isActive ? "Active" : "Inactive"}
                    </span>
                  </td>
                  <td className="px-6 py-4 text-right space-x-2">
                    <button className="text-slate-600 hover:text-slate-900">Edit</button>
                    <button className="text-red-600 hover:text-red-900">Delete</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
