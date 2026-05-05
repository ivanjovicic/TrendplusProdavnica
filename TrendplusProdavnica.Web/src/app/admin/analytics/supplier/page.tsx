"use client";

import { useEffect, useState, useCallback } from "react";
import { useRouter, useSearchParams, usePathname } from "next/navigation";
import {
  analyticsApi,
  SupplierSalesReport,
  SupplierSalesStats,
  ShoeTypeSalesReport,
} from "@/lib/api/analytics";
import { formatCurrency } from "@/lib/utils/format";

// ─── Types ────────────────────────────────────────────────────────────────────

type Tab = "overview" | "scorecard" | "assortment";

// ─── Helpers ──────────────────────────────────────────────────────────────────

function getCompletionRate(stat: SupplierSalesStats): number {
  if (stat.totalOrders === 0) return 0;
  return (stat.completedOrders / stat.totalOrders) * 100;
}

function getRevenueShare(stat: SupplierSalesStats, total: number): number {
  if (total === 0) return 0;
  return (stat.totalRevenue / total) * 100;
}

function getPerformanceTier(
  conversionRate: number,
  completionRate: number
): { label: string; cls: string } {
  if (conversionRate >= 3 && completionRate >= 75)
    return { label: "Top performer", cls: "bg-green-50 text-green-700 border border-green-200" };
  if (conversionRate >= 1.5 && completionRate >= 55)
    return { label: "Prosek", cls: "bg-yellow-50 text-yellow-700 border border-yellow-200" };
  return { label: "Pod nadzorom", cls: "bg-red-50 text-red-700 border border-red-200" };
}

// ─── Tab 1: Pregled — canonical supplier decision surface ─────────────────────

function OverviewTab({
  report,
  onBrandSelect,
}: {
  report: SupplierSalesReport;
  onBrandSelect: (brandId: number) => void;
}) {
  const sorted = [...report.suppliers].sort((a, b) => b.totalRevenue - a.totalRevenue);

  return (
    <div className="space-y-6">
      {/* Market summary */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div className="bg-white p-5 border border-slate-200 rounded-lg shadow-sm">
          <p className="text-xs text-slate-500 uppercase tracking-wider mb-1">Ukupan prihod tržišta</p>
          <p className="text-2xl font-semibold text-slate-900">
            {formatCurrency(report.totalMarketRevenue)}
          </p>
        </div>
        <div className="bg-white p-5 border border-slate-200 rounded-lg shadow-sm">
          <p className="text-xs text-slate-500 uppercase tracking-wider mb-1">Ukupne porudžbine</p>
          <p className="text-2xl font-semibold text-slate-900">{report.totalMarketOrders}</p>
        </div>
        <div className="bg-white p-5 border border-slate-200 rounded-lg shadow-sm">
          <p className="text-xs text-slate-500 uppercase tracking-wider mb-1">Dobavljači u izveštaju</p>
          <p className="text-2xl font-semibold text-slate-900">{report.totalSuppliersIncluded}</p>
        </div>
      </div>

      {/* Supplier table */}
      <div className="bg-white border border-slate-200 rounded-lg shadow-sm overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-left text-sm">
            <thead className="bg-slate-50 border-b border-slate-200">
              <tr>
                <th className="px-5 py-3 font-medium text-slate-700">Dobavljač</th>
                <th className="px-5 py-3 font-medium text-slate-700 text-right">Porudžbine</th>
                <th className="px-5 py-3 font-medium text-slate-700 text-right">Prihod</th>
                <th className="px-5 py-3 font-medium text-slate-700 text-right">AOV</th>
                <th className="px-5 py-3 font-medium text-slate-700 text-right">Jedinice</th>
                <th className="px-5 py-3 font-medium text-slate-700 text-right">Konverzija</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {sorted.map((stat) => (
                <tr
                  key={stat.brandId}
                  className="hover:bg-slate-50 transition-colors cursor-pointer"
                  onClick={() => onBrandSelect(stat.brandId)}
                  title="Klikni da filtruješ sve tabove na ovog dobavljača"
                >
                  <td className="px-5 py-3">
                    <div className="font-medium text-slate-900">{stat.brandName}</div>
                    <div className="text-xs text-slate-400">ID: {stat.brandId}</div>
                  </td>
                  <td className="px-5 py-3 text-right">
                    <div className="text-slate-900">{stat.totalOrders}</div>
                    <div className="text-xs text-slate-400">{stat.completedOrders} završenih</div>
                  </td>
                  <td className="px-5 py-3 text-right font-medium text-slate-900">
                    {formatCurrency(stat.totalRevenue)}
                  </td>
                  <td className="px-5 py-3 text-right text-slate-600">
                    {formatCurrency(stat.averageOrderValue)}
                  </td>
                  <td className="px-5 py-3 text-right text-slate-600">{stat.unitsOrdered}</td>
                  <td className="px-5 py-3 text-right">
                    <span
                      className={`px-2 py-0.5 rounded-full text-xs ${
                        stat.conversionRate > 2
                          ? "bg-green-50 text-green-700"
                          : "bg-slate-100 text-slate-600"
                      }`}
                    >
                      {stat.conversionRate.toFixed(2)}%
                    </span>
                  </td>
                </tr>
              ))}
              {sorted.length === 0 && (
                <tr>
                  <td colSpan={6} className="px-5 py-10 text-center text-slate-400">
                    Nema podataka za odabrani period i filter.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>

      <div className="text-xs text-slate-400 bg-slate-50 p-3 rounded">
        Klikni na red dobavljača da filtruješ sve tabove na tog dobavljača. Podaci su read-only snapshots.
      </div>
    </div>
  );
}

// ─── Tab 2: Scorecard — SupplierDecisionHub (quality & performance tiers) ─────

function ScorecardTab({ report }: { report: SupplierSalesReport }) {
  const totalRevenue = report.totalMarketRevenue;
  const sorted = [...report.suppliers].sort((a, b) => b.totalRevenue - a.totalRevenue);

  const tierCounts = sorted.reduce(
    (acc, stat) => {
      const completion = getCompletionRate(stat);
      const tier = getPerformanceTier(stat.conversionRate, completion);
      acc[tier.label] = (acc[tier.label] ?? 0) + 1;
      return acc;
    },
    {} as Record<string, number>
  );

  return (
    <div className="space-y-6">
      {/* Tier distribution summary */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        {[
          {
            label: "Top performer",
            borderCls: "border-green-200 bg-green-50",
            textCls: "text-green-700",
            note: "Konverzija ≥ 3% i završenost ≥ 75%",
          },
          {
            label: "Prosek",
            borderCls: "border-yellow-200 bg-yellow-50",
            textCls: "text-yellow-700",
            note: "Konverzija ≥ 1.5% i završenost ≥ 55%",
          },
          {
            label: "Pod nadzorom",
            borderCls: "border-red-200 bg-red-50",
            textCls: "text-red-700",
            note: "Ispod pragova konverzije ili završenosti",
          },
        ].map((tier) => (
          <div key={tier.label} className={`p-5 border rounded-lg shadow-sm ${tier.borderCls}`}>
            <p className={`text-xs uppercase tracking-wider mb-1 ${tier.textCls}`}>{tier.label}</p>
            <p className={`text-2xl font-semibold ${tier.textCls}`}>{tierCounts[tier.label] ?? 0}</p>
            <p className={`text-xs mt-1 opacity-70 ${tier.textCls}`}>{tier.note}</p>
          </div>
        ))}
      </div>

      {/* Scorecard table */}
      <div className="bg-white border border-slate-200 rounded-lg shadow-sm overflow-hidden">
        <div className="px-5 py-3 border-b border-slate-100 bg-slate-50">
          <p className="text-xs text-slate-500 uppercase tracking-wide font-medium">
            Performance scorecard — sortirano po prihodima
          </p>
        </div>
        <div className="overflow-x-auto">
          <table className="w-full text-left text-sm">
            <thead className="border-b border-slate-100">
              <tr>
                <th className="px-5 py-3 font-medium text-slate-600 w-10">#</th>
                <th className="px-5 py-3 font-medium text-slate-600">Dobavljač</th>
                <th className="px-5 py-3 font-medium text-slate-600 text-right">Udeo prihoda</th>
                <th className="px-5 py-3 font-medium text-slate-600 text-right">Konverzija</th>
                <th className="px-5 py-3 font-medium text-slate-600 text-right">Završenost</th>
                <th className="px-5 py-3 font-medium text-slate-600 text-right">Tier</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-50">
              {sorted.map((stat, idx) => {
                const completion = getCompletionRate(stat);
                const share = getRevenueShare(stat, totalRevenue);
                const tier = getPerformanceTier(stat.conversionRate, completion);
                return (
                  <tr key={stat.brandId} className="hover:bg-slate-50 transition-colors">
                    <td className="px-5 py-3 text-slate-400 text-xs">{idx + 1}</td>
                    <td className="px-5 py-3">
                      <div className="font-medium text-slate-900">{stat.brandName}</div>
                      <div className="text-xs text-slate-400">{formatCurrency(stat.totalRevenue)}</div>
                    </td>
                    <td className="px-5 py-3 text-right">
                      <div className="text-slate-900 mb-1">{share.toFixed(1)}%</div>
                      <div className="h-1 bg-slate-100 rounded w-full">
                        <div
                          className="h-1 bg-slate-500 rounded"
                          style={{ width: `${Math.min(share * 2, 100)}%` }}
                        />
                      </div>
                    </td>
                    <td className="px-5 py-3 text-right">
                      <span
                        className={`px-2 py-0.5 rounded-full text-xs ${
                          stat.conversionRate >= 3
                            ? "bg-green-50 text-green-700"
                            : stat.conversionRate >= 1.5
                            ? "bg-yellow-50 text-yellow-700"
                            : "bg-red-50 text-red-700"
                        }`}
                      >
                        {stat.conversionRate.toFixed(2)}%
                      </span>
                    </td>
                    <td className="px-5 py-3 text-right text-slate-600">
                      {completion.toFixed(0)}%
                    </td>
                    <td className="px-5 py-3 text-right">
                      <span className={`px-2 py-0.5 rounded-full text-xs border ${tier.cls}`}>
                        {tier.label}
                      </span>
                    </td>
                  </tr>
                );
              })}
              {sorted.length === 0 && (
                <tr>
                  <td colSpan={6} className="px-5 py-10 text-center text-slate-400">
                    Nema podataka za scorecard u odabranom periodu.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>

      <div className="text-xs text-slate-400 bg-slate-50 p-3 rounded">
        Scorecard tier se izračunava na osnovu stope konverzije i stope završenih porudžbina.
        Isti dataset kao Pregled tab — nema odvojenog API call-a.
      </div>
    </div>
  );
}

// ─── Tab 3: Asortiman — SupplierFootwearAnalytics (shoe-type breakdown) ────────

function AssortmentTab({
  report,
  isLoading,
}: {
  report: ShoeTypeSalesReport | null;
  isLoading: boolean;
}) {
  if (isLoading) {
    return <div className="p-8 text-center text-slate-500">Učitavanje asortimana...</div>;
  }

  if (!report) return null;

  const sorted = [...report.shoeTypes].sort((a, b) => b.totalRevenue - a.totalRevenue);
  const maxRevenue = sorted[0]?.totalRevenue ?? 1;

  return (
    <div className="space-y-6">
      {/* Assortment summary */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div className="bg-white p-5 border border-slate-200 rounded-lg shadow-sm">
          <p className="text-xs text-slate-500 uppercase tracking-wider mb-1">Prihod tržišta</p>
          <p className="text-2xl font-semibold text-slate-900">
            {formatCurrency(report.totalMarketRevenue)}
          </p>
        </div>
        <div className="bg-white p-5 border border-slate-200 rounded-lg shadow-sm">
          <p className="text-xs text-slate-500 uppercase tracking-wider mb-1">Ukupne porudžbine</p>
          <p className="text-2xl font-semibold text-slate-900">{report.totalMarketOrders}</p>
        </div>
        <div className="bg-white p-5 border border-slate-200 rounded-lg shadow-sm">
          <p className="text-xs text-slate-500 uppercase tracking-wider mb-1">Kategorije obuće</p>
          <p className="text-2xl font-semibold text-slate-900">{report.totalTypesIncluded}</p>
        </div>
      </div>

      {/* Shoe-type breakdown */}
      <div className="bg-white border border-slate-200 rounded-lg shadow-sm overflow-hidden">
        <div className="px-5 py-3 border-b border-slate-100 bg-slate-50">
          <p className="text-xs text-slate-500 uppercase tracking-wide font-medium">
            Asortiman po tipu obuće — sortirano po prihodima
          </p>
        </div>
        <div className="overflow-x-auto">
          <table className="w-full text-left text-sm">
            <thead className="border-b border-slate-100">
              <tr>
                <th className="px-5 py-3 font-medium text-slate-600">Kategorija</th>
                <th className="px-5 py-3 font-medium text-slate-600 text-right">Porudžbine</th>
                <th className="px-5 py-3 font-medium text-slate-600 text-right">Prihod</th>
                <th className="px-5 py-3 font-medium text-slate-600 text-right">AOV</th>
                <th className="px-5 py-3 font-medium text-slate-600 text-right">Konverzija</th>
                <th className="px-5 py-3 font-medium text-slate-600 text-right">Udeo</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-50">
              {sorted.map((stat) => {
                const share =
                  report.totalMarketRevenue > 0
                    ? (stat.totalRevenue / report.totalMarketRevenue) * 100
                    : 0;
                return (
                  <tr
                    key={stat.categoryId ?? "null"}
                    className="hover:bg-slate-50 transition-colors"
                  >
                    <td className="px-5 py-3">
                      <div className="font-medium text-slate-900">
                        {stat.categoryName ?? "Ostalo / Nepoznato"}
                      </div>
                      <div className="text-xs text-slate-400">ID: {stat.categoryId ?? "N/A"}</div>
                    </td>
                    <td className="px-5 py-3 text-right">
                      <div className="text-slate-900">{stat.totalOrders}</div>
                      <div className="text-xs text-slate-400">{stat.completedOrders} završenih</div>
                    </td>
                    <td className="px-5 py-3 text-right font-medium text-slate-900">
                      {formatCurrency(stat.totalRevenue)}
                    </td>
                    <td className="px-5 py-3 text-right text-slate-600">
                      {formatCurrency(stat.averageOrderValue)}
                    </td>
                    <td className="px-5 py-3 text-right">
                      <span
                        className={`px-2 py-0.5 rounded-full text-xs ${
                          stat.conversionRate > 2
                            ? "bg-green-50 text-green-700"
                            : "bg-slate-100 text-slate-600"
                        }`}
                      >
                        {stat.conversionRate.toFixed(2)}%
                      </span>
                    </td>
                    <td className="px-5 py-3 text-right">
                      <div className="text-slate-600 text-xs mb-1">{share.toFixed(1)}%</div>
                      <div className="h-1 bg-slate-100 rounded w-16 ml-auto">
                        <div
                          className="h-1 bg-slate-400 rounded"
                          style={{
                            width: `${Math.min((stat.totalRevenue / maxRevenue) * 100, 100)}%`,
                          }}
                        />
                      </div>
                    </td>
                  </tr>
                );
              })}
              {sorted.length === 0 && (
                <tr>
                  <td colSpan={6} className="px-5 py-10 text-center text-slate-400">
                    Nema podataka o asortimanu za odabrani period.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>

      <div className="text-xs text-slate-400 bg-slate-50 p-3 rounded">
        Asortiman prikazuje tržišni pregled prodaje po tipu obuće. Period filter iz zajedničkog
        filter bara važi i za ovaj tab.
      </div>
    </div>
  );
}

// ─── Main Component — Canonical Supplier Surface ──────────────────────────────

export default function SupplierAnalyticsPage() {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();

  // Derive all state from URL — single source of truth
  const activeTab = (searchParams.get("tab") as Tab | null) ?? "overview";
  const filterFrom = searchParams.get("from") ?? "";
  const filterTo = searchParams.get("to") ?? "";
  const filterBrandId = searchParams.get("brandId") ?? "";

  const [supplierReport, setSupplierReport] = useState<SupplierSalesReport | null>(null);
  const [assortmentReport, setAssortmentReport] = useState<ShoeTypeSalesReport | null>(null);
  const [isLoadingSupplier, setIsLoadingSupplier] = useState(true);
  const [isLoadingAssortment, setIsLoadingAssortment] = useState(false);
  const [supplierError, setSupplierError] = useState<string | null>(null);

  // ─── URL state helpers ──────────────────────────────────────────────────────

  const updateParam = useCallback(
    (key: string, value: string) => {
      const params = new URLSearchParams(searchParams.toString());
      if (value) params.set(key, value);
      else params.delete(key);
      router.replace(`${pathname}?${params.toString()}`);
    },
    [router, pathname, searchParams]
  );

  const setTab = useCallback(
    (tab: Tab) => updateParam("tab", tab),
    [updateParam]
  );

  const handleBrandSelect = useCallback(
    (brandId: number) => updateParam("brandId", brandId.toString()),
    [updateParam]
  );

  const resetFilters = useCallback(() => {
    const params = new URLSearchParams();
    params.set("tab", activeTab);
    router.replace(`${pathname}?${params.toString()}`);
  }, [router, pathname, activeTab]);

  // ─── Fetch supplier data (Pregled + Scorecard share this) ──────────────────

  useEffect(() => {
    setIsLoadingSupplier(true);
    setSupplierError(null);

    const params: Record<string, string | number> = { limit: 100 };
    if (filterFrom) params.from = filterFrom;
    if (filterTo) params.to = filterTo;
    if (filterBrandId) params.brandId = Number(filterBrandId);

    analyticsApi
      .getSupplierSalesStats(params)
      .then(setSupplierReport)
      .catch((err: { message?: string }) =>
        setSupplierError(err.message ?? "Greška pri učitavanju supplier podataka")
      )
      .finally(() => setIsLoadingSupplier(false));
  }, [filterFrom, filterTo, filterBrandId]);

  // ─── Fetch assortment data (lazy — only when Asortiman tab is active) ──────

  useEffect(() => {
    if (activeTab !== "assortment") return;

    setIsLoadingAssortment(true);

    const params: Record<string, string | number> = { limit: 50 };
    if (filterFrom) params.from = filterFrom;
    if (filterTo) params.to = filterTo;

    analyticsApi
      .getShoeTypeSalesStats(params)
      .then(setAssortmentReport)
      .catch(() => {
        // Non-critical: assortment tab shows empty state on error
      })
      .finally(() => setIsLoadingAssortment(false));
  }, [activeTab, filterFrom, filterTo]);

  // ─── Derived ────────────────────────────────────────────────────────────────

  const hasActiveFilters = filterFrom || filterTo || filterBrandId;

  const TABS: { key: Tab; label: string; hint: string }[] = [
    { key: "overview", label: "Pregled", hint: "canonical" },
    { key: "scorecard", label: "Scorecard", hint: "quality / tiers" },
    { key: "assortment", label: "Asortiman", hint: "shoe-type drilldown" },
  ];

  // ─── Render ─────────────────────────────────────────────────────────────────

  return (
    <div className="space-y-6 max-w-7xl mx-auto">
      {/* Page header */}
      <div className="flex justify-between items-end border-b border-slate-200 pb-4">
        <div>
          <h1 className="text-3xl font-light text-slate-900 mb-1">Analitika dobavljača</h1>
          <p className="text-slate-500 text-sm">
            Centralni supplier decision surface — pregled, scorecard i asortiman
          </p>
        </div>
        {supplierReport && (
          <div className="text-right text-xs text-slate-400">
            <p>
              Generisano:{" "}
              {new Date(supplierReport.reportGeneratedAtUtc).toLocaleString("sr-RS")}
            </p>
            <p>
              Period:{" "}
              {new Date(supplierReport.periodStart).toLocaleDateString("sr-RS")} –{" "}
              {new Date(supplierReport.periodEnd).toLocaleDateString("sr-RS")}
            </p>
          </div>
        )}
      </div>

      {/* Shared filter bar — applies to ALL tabs */}
      <div className="bg-white border border-slate-200 rounded-lg p-4 flex flex-wrap items-end gap-4">
        <div className="flex flex-col gap-1">
          <label className="text-xs text-slate-500 uppercase tracking-wide">Od</label>
          <input
            type="date"
            value={filterFrom}
            onChange={(e) => updateParam("from", e.target.value)}
            className="border border-slate-200 rounded px-3 py-1.5 text-sm text-slate-800 focus:outline-none focus:ring-2 focus:ring-slate-300"
          />
        </div>
        <div className="flex flex-col gap-1">
          <label className="text-xs text-slate-500 uppercase tracking-wide">Do</label>
          <input
            type="date"
            value={filterTo}
            onChange={(e) => updateParam("to", e.target.value)}
            className="border border-slate-200 rounded px-3 py-1.5 text-sm text-slate-800 focus:outline-none focus:ring-2 focus:ring-slate-300"
          />
        </div>
        <div className="flex flex-col gap-1">
          <label className="text-xs text-slate-500 uppercase tracking-wide">Dobavljač ID</label>
          <input
            type="number"
            value={filterBrandId}
            placeholder="Svi"
            min={1}
            onChange={(e) => updateParam("brandId", e.target.value)}
            className="border border-slate-200 rounded px-3 py-1.5 text-sm text-slate-800 w-28 focus:outline-none focus:ring-2 focus:ring-slate-300"
          />
        </div>
        {hasActiveFilters && (
          <button
            onClick={resetFilters}
            className="px-3 py-1.5 text-xs text-slate-500 border border-slate-200 rounded hover:bg-slate-50 transition"
          >
            Resetuj filtere
          </button>
        )}
        <div className="ml-auto text-xs text-slate-400 italic">
          Filteri važe za sve tabove
        </div>
      </div>

      {/* Tab navigation */}
      <div className="border-b border-slate-200">
        <div className="flex">
          {TABS.map((tab) => (
            <button
              key={tab.key}
              onClick={() => setTab(tab.key)}
              className={`px-6 py-3 text-sm font-medium border-b-2 transition-all ${
                activeTab === tab.key
                  ? "border-slate-900 text-slate-900"
                  : "border-transparent text-slate-500 hover:text-slate-700 hover:border-slate-300"
              }`}
            >
              {tab.label}
              {tab.key === "overview" && (
                <span className="ml-2 text-xs text-slate-400 font-normal hidden md:inline">
                  canonical
                </span>
              )}
            </button>
          ))}
        </div>
      </div>

      {/* Error state */}
      {supplierError && (
        <div className="p-4 bg-red-50 border border-red-200 rounded text-red-600 text-sm">
          {supplierError}
        </div>
      )}

      {/* Loading state for supplier data */}
      {isLoadingSupplier && !supplierError && (
        <div className="p-8 text-center text-slate-500">Učitavanje podataka o dobavljačima...</div>
      )}

      {/* Tab 1 — Pregled (canonical) */}
      {!isLoadingSupplier && activeTab === "overview" && supplierReport && (
        <OverviewTab report={supplierReport} onBrandSelect={handleBrandSelect} />
      )}

      {/* Tab 2 — Scorecard (SupplierDecisionHub) */}
      {!isLoadingSupplier && activeTab === "scorecard" && supplierReport && (
        <ScorecardTab report={supplierReport} />
      )}

      {/* Tab 3 — Asortiman (SupplierFootwearAnalytics) */}
      {!isLoadingSupplier && activeTab === "assortment" && (
        <AssortmentTab report={assortmentReport} isLoading={isLoadingAssortment} />
      )}
    </div>
  );
}
