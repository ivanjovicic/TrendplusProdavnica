"use client";

import { useEffect, useState } from "react";
import { adminApiClient, AdminListResponse, EditorialListItem } from "@/lib/admin/client";
import { useAdmin } from "@/lib/admin/context";

export default function AdminEditorialPage() {
  const { isAuthenticated } = useAdmin();
  const [articles, setArticles] = useState<AdminListResponse<EditorialListItem> | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [publishedFilter, setPublishedFilter] = useState<boolean | undefined>(undefined);
  const [pageSize] = useState(20);

  useEffect(() => {
    if (!isAuthenticated) return;

    const loadArticles = async () => {
      try {
        setIsLoading(true);
        const result = await adminApiClient.getEditorialArticles(
          1,
          pageSize,
          publishedFilter
        );
        setArticles(result);
      } catch (error) {
        console.error("Failed to load articles:", error);
      } finally {
        setIsLoading(false);
      }
    };

    loadArticles();
  }, [isAuthenticated, pageSize, publishedFilter]);

  if (isLoading) {
    return <div className="text-slate-600">Loading articles...</div>;
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-light">Editorial</h1>
        <button className="px-4 py-2 bg-slate-900 text-white rounded hover:bg-slate-800 transition">
          + New Article
        </button>
      </div>

      {/* Filters */}
      <div>
        <select
          value={publishedFilter === undefined ? "" : publishedFilter.toString()}
          onChange={(e) => {
            if (e.target.value === "") {
              setPublishedFilter(undefined);
            } else {
              setPublishedFilter(e.target.value === "true");
            }
          }}
          className="px-4 py-2 border border-slate-300 rounded focus:outline-none focus:ring-2 focus:ring-slate-900"
        >
          <option value="">All Articles</option>
          <option value="true">Published</option>
          <option value="false">Draft</option>
        </select>
      </div>

      <div className="bg-white border border-slate-200 rounded overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead className="bg-slate-50 border-b border-slate-200">
              <tr>
                <th className="px-6 py-4 text-left font-medium text-slate-700">Title</th>
                <th className="px-6 py-4 text-left font-medium text-slate-700">Views</th>
                <th className="px-6 py-4 text-left font-medium text-slate-700">
                  Status
                </th>
                <th className="px-6 py-4 text-left font-medium text-slate-700">Date</th>
                <th className="px-6 py-4 text-left font-medium text-slate-700">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody>
              {articles?.items.map((article) => (
                <tr
                  key={article.id}
                  className="border-b border-slate-100 hover:bg-slate-50"
                >
                  <td className="px-6 py-4 font-medium">{article.title}</td>
                  <td className="px-6 py-4 text-slate-600">{article.viewCount || 0}</td>
                  <td className="px-6 py-4">
                    <span
                      className={`inline-block px-2 py-1 rounded text-xs font-medium ${
                        article.isPublished
                          ? "bg-green-50 text-green-700"
                          : "bg-slate-100 text-slate-700"
                      }`}
                    >
                      {article.isPublished ? "Published" : "Draft"}
                    </span>
                  </td>
                  <td className="px-6 py-4 text-slate-600">
                    {new Date(article.publishedUtc).toLocaleDateString()}
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
