import React, { createContext, useContext, useState, useEffect } from 'react';
import { LoginRequest, LoginResponse } from '../types';
import { apiClient } from '../api-client';

interface AuthContextType {
  isAuthenticated: boolean;
  accountId: string | null;
  accountName: string | null;
  error: string | null;
  login: (credentials: LoginRequest) => Promise<void>;
  logout: () => void;
  clearError: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({
  children,
}) => {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [accountId, setAccountId] = useState<string | null>(null);
  const [accountName, setAccountName] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const token = localStorage.getItem('authToken');
    const storedAccountId = localStorage.getItem('accountId');
    const storedAccountName = localStorage.getItem('accountName');

    if (token && storedAccountId && storedAccountName) {
      setIsAuthenticated(true);
      setAccountId(storedAccountId);
      setAccountName(storedAccountName);
    }
  }, []);

  const login = async (credentials: LoginRequest): Promise<void> => {
    try {
      setError(null);
      const response: LoginResponse = await apiClient.login(credentials);

      localStorage.setItem('authToken', response.token);
      localStorage.setItem('accountId', response.accountId);
      localStorage.setItem('accountName', response.accountName);

      setIsAuthenticated(true);
      setAccountId(response.accountId);
      setAccountName(response.accountName);
    } catch (err: any) {
      const errorMessage = err.message || 'Login failed';
      setError(errorMessage);
      throw err;
    }
  };

  const logout = (): void => {
    localStorage.removeItem('authToken');
    localStorage.removeItem('accountId');
    localStorage.removeItem('accountName');
    apiClient.clearToken();

    setIsAuthenticated(false);
    setAccountId(null);
    setAccountName(null);
    setError(null);
  };

  const clearError = (): void => {
    setError(null);
  };

  const value: AuthContextType = {
    isAuthenticated,
    accountId,
    accountName,
    error,
    login,
    logout,
    clearError,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export const useAuth = (): AuthContextType => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
