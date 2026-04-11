"use client";

import { useAdmin } from "@/lib/admin/context";
import { useRouter, usePathname } from "next/navigation";
import { useEffect } from "react";
import Link from "next/link";
import { AdminProvider } from "@/lib/admin/context";

const SIDEBAR_ITEMS = [
  {
    label: "Dashboard",
    href: "/admin",
    icon: "📊",
  },
  {
    label: "Products",
    href: "/admin/products",
    icon: "📦",
  },
  {
    label: "Orders",
    href: "/admin/orders",
    icon: "📋",
  },
  {
    label: "Brands",
    href: "/admin/brands",
    icon: "🏷️",
  },
  {
    label: "Collections",
    href: "/admin/collections",
    icon: "📚",
  },
  {
    label: "Editorial",
    href: "/admin/editorial",
    icon: "✍️",
  },
  {
    label: "Homepage",
    href: "/admin/homepage",
    icon: "🏠",
  },
];

function AdminShell({
  children,
}: {
  children: React.ReactNode;
}) {
  const { isAuthenticated, isLoading, user, logout } = useAdmin();
  const router = useRouter();
  const pathname = usePathname();

  useEffect(() => {
    if (!isLoading && !isAuthenticated && pathname !== "/admin/login") {
      router.push("/admin/login");
    }
  }, [isAuthenticated, isLoading, router, pathname]);

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <p className="text-slate-600">Loading...</p>
      </div>
    );
  }

  if (!isAuthenticated) {
    return null;
  }

  const handleLogout = () => {
    logout();
    router.push("/admin/login");
  };

  return (
    <div className="flex h-screen bg-slate-50">
      {/* Sidebar */}
      <aside className="w-64 bg-slate-900 text-white p-6 flex flex-col">
        <div className="mb-8">
          <h1 className="text-2xl font-light tracking-wider">TRENDPLUS</h1>
          <p className="text-slate-400 text-sm">Admin Panel</p>
        </div>

        <nav className="flex-1 space-y-2">
          {SIDEBAR_ITEMS.map((item) => (
            <Link
              key={item.href}
              href={item.href}
              className={`flex items-center gap-3 px-4 py-3 rounded transition ${
                pathname === item.href
                  ? "bg-slate-700 text-white"
                  : "text-slate-300 hover:bg-slate-800"
              }`}
            >
              <span className="text-lg">{item.icon}</span>
              <span>{item.label}</span>
            </Link>
          ))}
        </nav>

        {/* User info and logout */}
        <div className="border-t border-slate-700 pt-6">
          <div className="text-sm mb-4">
            <p className="text-slate-300">Logged in as</p>
            <p className="font-medium">{user?.fullName}</p>
          </div>
          <button
            onClick={handleLogout}
            className="w-full px-4 py-2 bg-slate-700 hover:bg-slate-600 rounded transition"
          >
            Logout
          </button>
        </div>
      </aside>

      {/* Main content */}
      <main className="flex-1 flex flex-col overflow-hidden">
        {/* Top bar */}
        <header className="bg-white border-b border-slate-200 px-8 py-4 flex items-center justify-between">
          <h2 className="text-xl font-light text-slate-900">
            {SIDEBAR_ITEMS.find(
              (item) =>
                item.href === pathname ||
                pathname.startsWith(item.href + "/")
            )?.label || "Admin"}
          </h2>
          <div className="text-sm text-slate-600">
            {new Date().toLocaleDateString()}
          </div>
        </header>

        {/* Content area */}
        <div className="flex-1 overflow-auto p-8">{children}</div>
      </main>
    </div>
  );
}

export default function AdminLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <AdminProvider>
      <AdminShell>{children}</AdminShell>
    </AdminProvider>
  );
}
