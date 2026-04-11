"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { useAdmin } from "@/lib/admin/context";

export default function AdminLoginPage() {
  const [email, setEmail] = useState("admin@trendplus.com");
  const [password, setPassword] = useState("admin123!@#");
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const router = useRouter();
  const { login } = useAdmin();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    setIsLoading(true);

    try {
      await login(email, password);
      router.push("/admin");
    } catch (err) {
      setError("Invalid email or password");
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-slate-50">
      <div className="w-full max-w-md">
        <div className="bg-white shadow rounded-lg p-8">
          <h1 className="text-3xl font-light text-center mb-2">TRENDPLUS</h1>
          <p className="text-center text-slate-600 mb-8">Admin Panel</p>

          {error && (
            <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded mb-6">
              {error}
            </div>
          )}

          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-2">
                Email
              </label>
              <input
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                disabled={isLoading}
                className="w-full px-4 py-2 border border-slate-300 rounded focus:outline-none focus:ring-2 focus:ring-slate-900 disabled:bg-slate-50"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-slate-700 mb-2">
                Password
              </label>
              <input
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                disabled={isLoading}
                className="w-full px-4 py-2 border border-slate-300 rounded focus:outline-none focus:ring-2 focus:ring-slate-900 disabled:bg-slate-50"
              />
            </div>

            <button
              type="submit"
              disabled={isLoading}
              className="w-full bg-slate-900 text-white py-2 rounded font-medium hover:bg-slate-800 disabled:bg-slate-400 transition"
            >
              {isLoading ? "Signing in..." : "Sign In"}
            </button>
          </form>

          <div className="mt-8 p-4 bg-slate-50 rounded text-sm text-slate-600">
            <p className="font-medium mb-2">Demo Credentials:</p>
            <p>Email: admin@trendplus.com</p>
            <p>Password: admin123!@#</p>
          </div>
        </div>
      </div>
    </div>
  );
}
