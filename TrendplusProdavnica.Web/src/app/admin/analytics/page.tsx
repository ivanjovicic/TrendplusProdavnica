"use client";

import { useEffect, useState } from "react";
import { analyticsApi, ShoeTypeSalesReport, ShoeTypeSalesStats } from "@/lib/api/analytics";
import { formatCurrency } from "@/lib/utils/format";

export default function ShoeTypeAnalyticsPage() {
  const [report, setReport] = useState<ShoeTypeSalesReport | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchStats = async () => {
      try {
        setIsLoading(true);
        const data = await analyticsApi.getShoeTypeSalesStats({
          limit: 50
        });
        setReport(data);
      } catch (err: any) {
        console.error("Failed to fetch analytics:", err);
        setError(err.message || "Greška pri učitavanju analitike");
      } finally {
        setIsLoading(false);
      }
    };

    fetchStats();
  }, []);

  if (isLoading) {
    return <div className="p-8 text-center text-slate-500">Učitavanje analitike po tipu obuće...</div>;
  }

  if (error) {
    return <div className="p-8 text-center text-red-500">{error}</div>;
  }

  return (
    <div className="p-8 space-y-8 max-w-7xl mx-auto">
      <div className="flex justify-between items-end border-b border-slate-200 pb-4">
        <div>
          <h1 className="text-3xl font-light text-slate-900 mb-2">Prodaja po tipu obuće</h1>
          <p className="text-slate-500">Analiza prodaje i konverzije grupisanih po kategorijama (Shoe Types)</p>
        </div>
        <div className="text-right text-xs text-slate-400">
          <p>Generisano: {report ? new Date(report.reportGeneratedAtUtc).toLocaleString() : 'N/A'}</p>
          <p>Period: {report ? `${new Date(report.periodStart).toLocaleDateString()} - ${new Date(report.periodEnd).toLocaleDateString()}` : 'N/A'}</p>
        </div>
      </div>

      {/* Market Summary Cards */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div className="bg-white p-6 border border-slate-200 rounded-lg shadow-sm">
          <p className="text-sm text-slate-500 uppercase tracking-wider mb-1">Ukupan Prihod Tržišta</p>
          <p className="text-2xl font-semibold text-slate-900">{formatCurrency(report?.totalMarketRevenue ?? 0)}</p>
        </div>
        <div className="bg-white p-6 border border-slate-200 rounded-lg shadow-sm">
          <p className="text-sm text-slate-500 uppercase tracking-wider mb-1">Ukupne Porudžbine</p>
          <p className="text-2xl font-semibold text-slate-900">{report?.totalMarketOrders}</p>
        </div>
        <div className="bg-white p-6 border border-slate-200 rounded-lg shadow-sm">
          <p className="text-sm text-slate-500 uppercase tracking-wider mb-1">Broj Kategorija</p>
          <p className="text-2xl font-semibold text-slate-900">{report?.totalTypesIncluded}</p>
        </div>
      </div>

      {/* Main Stats Table */}
      <div className="bg-white border border-slate-200 rounded-lg shadow-sm overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-left text-sm">
            <thead className="bg-slate-50 border-b border-slate-200">
              <tr>
                <th className="px-6 py-4 font-medium text-slate-700">Kategorija</th>
                <th className="px-6 py-4 font-medium text-slate-700 text-right">Porudžbine</th>
                <th className="px-6 py-4 font-medium text-slate-700 text-right">Prihod</th>
                <th className="px-6 py-4 font-medium text-slate-700 text-right">AOV</th>
                <th className="px-6 py-4 font-medium text-slate-700 text-right">Jedinice</th>
                <th className="px-6 py-4 font-medium text-slate-700 text-right">Posete</th>
                <th className="px-6 py-4 font-medium text-slate-700 text-right">Konverzija</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {report?.shoeTypes.map((stat) => (
                <tr key={stat.categoryId ?? 'null'} className="hover:bg-slate-50 transition-colors">
                  <td className="px-6 py-4">
                    <div className="font-medium text-slate-900">{stat.categoryName || 'Ostalo/Nepoznato'}</div>
                    <div className="text-xs text-slate-400">ID: {stat.categoryId ?? 'N/A'}</div>
                  </td>
                  <td className="px-6 py-4 text-right">
                    <div className="text-slate-900">{stat.totalOrders}</div>
                    <div className="text-xs text-slate-400">
                      {stat.completedOrders} završenih
                    </div>
                  </td>
                  <td className="px-6 py-4 text-right font-medium text-slate-900">
                    {formatCurrency(stat.totalRevenue)}
                  </td>
                  <td className="px-6 py-4 text-right text-slate-600">
                    {formatCurrency(stat.averageOrderValue)}
                  </td>
                  <td className="px-6 py-4 text-right text-slate-600">
                    {stat.unitsOrdered}
                  </td>
                  <td className="px-6 py-4 text-right text-slate-600">
                    {stat.productViews}
                  </td>
                  <td className="px-6 py-4 text-right">
                    <div className="flex items-center justify-end gap-2">
                       <span className={`px-2 py-0.5 rounded-full text-xs ${
                         stat.conversionRate > 2 ? 'bg-green-50 text-green-700' : 'bg-slate-100 text-slate-600'
                       }`}>
                         {stat.conversionRate.toFixed(2)}%
                       </span>
                    </div>
                  </td>
                </tr>
              ))}
              {report?.shoeTypes.length === 0 && (
                <tr>
                  <td colSpan={7} className="px-6 py-12 text-center text-slate-400">
                    Nema podataka za prikaz u odabranom periodu.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>

      <div className="text-xs text-slate-400 bg-slate-50 p-4 rounded-lg">
        <p className="font-medium mb-1 uppercase tracking-tighter">Podržana tačnost i nepromenjivost:</p>
        <p>Ovaj izveštaj koristi read-only snapshots sačuvanih kategorija u trenutku kupovine (Historical Immutability). Verzija algoritma: {report?.shoeTypes[0]?.dataVersion || '1.0'}. Podaci se osvežavaju u realnom vremenu na osnovu transakcionih zapisa.</p>
      </div>
    </div>
  );
}
