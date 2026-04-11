"use client";

import React, { createContext, useContext, useState, useCallback, useEffect } from "react";
import { adminApiClient, AdminUser } from "@/lib/admin/client";

interface AdminContextType {
  user: AdminUser | null;
  token: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<void>;
  logout: () => void;
  refreshToken: (refreshToken: string) => Promise<void>;
}

const AdminContext = createContext<AdminContextType | undefined>(undefined);

export function AdminProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<AdminUser | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  // Initialize from localStorage on mount
  useEffect(() => {
    const storedToken = adminApiClient.getToken();
    if (storedToken) {
      setToken(storedToken);
      // In production, validate token with backend
    }
    setIsLoading(false);
  }, []);

  const login = useCallback(async (email: string, password: string) => {
    try {
      setIsLoading(true);
      const result = await adminApiClient.login(email, password);
      setToken(result.token);
      setUser(result.user);
    } catch (error) {
      throw error;
    } finally {
      setIsLoading(false);
    }
  }, []);

  const logout = useCallback(() => {
    setUser(null);
    setToken(null);
    adminApiClient.setToken(null);
  }, []);

  const refreshTokenFn = useCallback(async (refreshToken: string) => {
    try {
      const result = await adminApiClient.refreshToken(refreshToken);
      setToken(result.token);
      setUser(result.user);
    } catch (error) {
      logout();
      throw error;
    }
  }, [logout]);

  const value: AdminContextType = {
    user,
    token,
    isAuthenticated: !!token && !!user,
    isLoading,
    login,
    logout,
    refreshToken: refreshTokenFn,
  };

  return (
    <AdminContext.Provider value={value}>{children}</AdminContext.Provider>
  );
}

export function useAdmin(): AdminContextType {
  const context = useContext(AdminContext);
  if (!context) {
    throw new Error("useAdmin must be used within AdminProvider");
  }
  return context;
}
