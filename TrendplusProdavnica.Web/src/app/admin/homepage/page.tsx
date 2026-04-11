"use client";

import { useEffect, useState } from "react";
import { adminApiClient } from "@/lib/admin/client";
import { useAdmin } from "@/lib/admin/context";

export default function AdminHomePagePage() {
  const { isAuthenticated } = useAdmin();
  const [config, setConfig] = useState<any>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    if (!isAuthenticated) return;

    const loadConfig = async () => {
      try {
        setIsLoading(true);
        const result = await adminApiClient.getHomepageConfig();
        setConfig(result);
      } catch (error) {
        console.error("Failed to load homepage config:", error);
      } finally {
        setIsLoading(false);
      }
    };

    loadConfig();
  }, [isAuthenticated]);

  if (isLoading) {
    return <div className="text-slate-600">Loading homepage configuration...</div>;
  }

  return (
    <div className="space-y-6">
      <h1 className="text-3xl font-light">Homepage Configuration</h1>

      <div className="bg-white border border-slate-200 rounded p-6">
        <h2 className="text-xl font-light mb-6">Sections</h2>

        <div className="space-y-4">
          {config?.sections?.map((section: any, index: number) => (
            <div
              key={section.id}
              className="flex items-center justify-between p-4 border border-slate-200 rounded"
            >
              <div>
                <p className="font-medium">{section.type}</p>
                <p className="text-sm text-slate-600">Position: {index + 1}</p>
              </div>
              <div className="space-x-2">
                <button className="px-3 py-1 text-sm border border-slate-300 rounded hover:bg-slate-50">
                  Edit
                </button>
                <button className="px-3 py-1 text-sm border border-slate-300 rounded hover:bg-slate-50">
                  {section.isActive ? "Disable" : "Enable"}
                </button>
              </div>
            </div>
          ))}
        </div>

        <button className="mt-6 px-4 py-2 bg-slate-900 text-white rounded hover:bg-slate-800 transition">
          + Add Section
        </button>

        <div className="mt-8 p-4 bg-slate-50 rounded text-sm text-slate-600">
          <p className="font-medium mb-2">Supported Section Types:</p>
          <ul className="list-disc list-inside space-y-1">
            <li>hero - Hero banner with large title</li>
            <li>featured_products - Featured products grid</li>
            <li>categories - Category grid</li>
            <li>brands - Brand logo wall</li>
            <li>editorial - Featured articles</li>
            <li>trust_benefits - Trust/shipping benefits</li>
            <li>newsletter - Newsletter signup</li>
          </ul>
        </div>
      </div>
    </div>
  );
}
