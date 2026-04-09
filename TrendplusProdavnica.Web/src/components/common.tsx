import Link from 'next/link';
import type { BreadcrumbItemDto } from '@/lib/types';

interface BreadcrumbsProps {
  items: BreadcrumbItemDto[];
}

export function Breadcrumbs({ items }: BreadcrumbsProps) {
  return (
    <nav className="flex text-sm gap-2 mb-6">
      {items.map((item, idx) => (
        <div key={item.slug} className="flex items-center gap-2">
          {idx > 0 && <span className="text-gray-400">/</span>}
          {idx === items.length - 1 ? (
            <span className="text-gray-900 font-semibold">{item.label}</span>
          ) : (
            <Link href={item.url} className="text-blue-600 hover:underline">
              {item.label}
            </Link>
          )}
        </div>
      ))}</nav>
  );
}

interface PaginationProps {
  page: number;
  pageSize: number;
  totalCount: number;
  onPageChange: (page: number) => void;
}

export function Pagination({ page, pageSize, totalCount, onPageChange }: PaginationProps) {
  const pageCount = Math.ceil(totalCount / pageSize);
  
  if (pageCount <= 1) return null;

  return (
    <div className="flex justify-center gap-2 mt-8">
      {page > 1 && (
        <button onClick={() => onPageChange(page - 1)} className="px-3 py-1 border rounded hover:bg-gray-100">
          Prethodna
        </button>
      )}
      {Array.from({ length: pageCount }, (_, i) => i + 1)
        .filter((p) => Math.abs(p - page) <= 1 || p === 1 || p === pageCount)
        .map((p, idx, arr) => (
          <div key={p}>
            {idx > 0 && arr[idx - 1] !== p - 1 && <span className="px-2">...</span>}
            <button
              onClick={() => onPageChange(p)}
              className={`px-3 py-1 border rounded ${page === p ? 'bg-black text-white' : 'hover:bg-gray-100'}`}
            >
              {p}
            </button>
          </div>
        ))}
      {page < pageCount && (
        <button onClick={() => onPageChange(page + 1)} className="px-3 py-1 border rounded hover:bg-gray-100">
          Sledeća
        </button>
      )}
    </div>
  );
}

export function EmptyState() {
  return (
    <div className="text-center py-12">
      <p className="text-gray-600 mb-4">Nema rezultata</p>
      <Link href="/" className="text-blue-600 hover:underline">
        Nazad na početnu
      </Link>
    </div>
  );
}

export function LoadingState() {
  return (
    <div className="animate-pulse space-y-6">
      {Array.from({ length: 4 }).map((_, i) => (
        <div key={i} className="h-48 bg-gray-200 rounded" />
      ))}
    </div>
  );
}
