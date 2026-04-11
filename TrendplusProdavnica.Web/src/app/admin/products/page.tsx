"use client";

import { useEffect, useState } from "react";
import { adminApiClient, AdminListResponse, ProductListItem } from "@/lib/admin/client";
import { useAdmin } from "@/lib/admin/context";
import Link from "next/link";

export default function AdminProductsPage() {
  const { isAuthenticated } = useAdmin();
  const [products, setProducts] = useState<AdminListResponse<ProductListItem> | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [search, setSearch] = useState("");
  const [pageSize] = useState(20);

  useEffect(() => {
    if (!isAuthenticated) return;

    const loadProducts = async () => {
      try {
        setIsLoading(true);
        const result = await adminApiClient.getProducts(1, pageSize, search || undefined);
        setProducts(result);
      } catch (error) {
        console.error("Failed to load products:", error);
      } finally {
        setIsLoading(false);
      }
    };

    loadProducts();
  }, [isAuthenticated, pageSize, search]);

  const handleDelete = async (id: number) => {
    if (!confirm("Are you sure you want to delete this product?")) return;

    try {
      await adminApiClient.deleteProduct(id);
      setProducts(
        products
          ? {
              ...products,
              items: products.items.filter((p) => p.id !== id),
            }
          : null
      );
    } catch (error) {
      console.error("Failed to delete product:", error);
    }
  };

  if (isLoading) {
    return <div className="text-slate-600">Loading products...</div>;
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-light">Products</h1>
        <Link
          href="/admin/products/new"
          className="px-4 py-2 bg-slate-900 text-white rounded hover:bg-slate-800 transition"
        >
          + New Product
        </Link>
      </div>

      {/* Search */}
      <div>
        <input
          type="text"
          placeholder="Search products..."
          value={search}
          onChange={(e) => {
            setSearch(e.target.value);
          }}
          className="w-full px-4 py-2 border border-slate-300 rounded focus:outline-none focus:ring-2 focus:ring-slate-900"
        />
      </div>

      {/* Products Table */}
      <div className="bg-white border border-slate-200 rounded overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead className="bg-slate-50 border-b border-slate-200">
              <tr>
                <th className="px-6 py-4 text-left font-medium text-slate-700">
                  Name
                </th>
                <th className="px-6 py-4 text-left font-medium text-slate-700">
                  Brand
                </th>
                <th className="px-6 py-4 text-left font-medium text-slate-700">
                  Price
                </th>
                <th className="px-6 py-4 text-left font-medium text-slate-700">
                  Stock
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
              {products?.items.map((product) => (
                <tr
                  key={product.id}
                  className="border-b border-slate-100 hover:bg-slate-50"
                >
                  <td className="px-6 py-4 font-medium">{product.name}</td>
                  <td className="px-6 py-4 text-slate-600">{product.brandName}</td>
                  <td className="px-6 py-4">${product.price.toFixed(2)}</td>
                  <td className="px-6 py-4">
                    <span
                      className={
                        product.stockQuantity > 0
                          ? "text-green-700"
                          : "text-red-700"
                      }
                    >
                      {product.stockQuantity}
                    </span>
                  </td>
                  <td className="px-6 py-4">
                    <span
                      className={`inline-block px-2 py-1 rounded text-xs font-medium ${
                        product.isActive
                          ? "bg-green-50 text-green-700"
                          : "bg-slate-100 text-slate-700"
                      }`}
                    >
                      {product.isActive ? "Active" : "Inactive"}
                    </span>
                  </td>
                  <td className="px-6 py-4 text-right space-x-2">
                    <Link
                      href={`/admin/products/${product.id}`}
                      className="text-slate-600 hover:text-slate-900"
                    >
                      Edit
                    </Link>
                    <button
                      onClick={() => handleDelete(product.id)}
                      className="text-red-600 hover:text-red-900"
                    >
                      Delete
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        {/* Pagination */}
        <div className="px-6 py-4 border-t border-slate-200 flex items-center justify-between">
          <div className="text-sm text-slate-600">
            <span className="text-slate-600">{products?.totalCount || 0} total items</span>
          </div>
          <div className="space-x-2">
            {/* Pagination buttons disabled for now */}
          </div>
        </div>
      </div>
    </div>
  );
}
