import { renderHook, act, waitFor } from '@testing-library/react';
import { AuthProvider, useAuth } from '../contexts/AuthContext';
import { apiClient } from '../api-client';

jest.mock('../api-client');

describe('AuthContext', () => {
  beforeEach(() => {
    localStorage.clear();
    jest.clearAllMocks();
  });

  describe('login', () => {
    it('should call apiClient.login with credentials', async () => {
      const mockLogin = jest.fn().mockResolvedValue({
        token: 'test-token',
        accountId: 'account-123',
        accountName: 'Test Account',
      });
      (apiClient.login as jest.Mock) = mockLogin;

      const wrapper = ({ children }: { children: React.ReactNode }) => (
        <AuthProvider>{children}</AuthProvider>
      );
      const { result } = renderHook(() => useAuth(), { wrapper });

      await act(async () => {
        await result.current.login({ username: 'user', password: 'pass' });
      });

      expect(mockLogin).toHaveBeenCalledWith({
        username: 'user',
        password: 'pass',
      });
    });

    it('should store token in localStorage on successful login', async () => {
      const mockLogin = jest.fn().mockResolvedValue({
        token: 'test-token',
        accountId: 'account-123',
        accountName: 'Test Account',
      });
      (apiClient.login as jest.Mock) = mockLogin;

      const wrapper = ({ children }: { children: React.ReactNode }) => (
        <AuthProvider>{children}</AuthProvider>
      );
      const { result } = renderHook(() => useAuth(), { wrapper });

      await act(async () => {
        await result.current.login({ username: 'user', password: 'pass' });
      });

      expect(localStorage.getItem('authToken')).toBe('test-token');
    });

    it('should set isAuthenticated to true on successful login', async () => {
      const mockLogin = jest.fn().mockResolvedValue({
        token: 'test-token',
        accountId: 'account-123',
        accountName: 'Test Account',
      });
      (apiClient.login as jest.Mock) = mockLogin;

      const wrapper = ({ children }: { children: React.ReactNode }) => (
        <AuthProvider>{children}</AuthProvider>
      );
      const { result } = renderHook(() => useAuth(), { wrapper });

      expect(result.current.isAuthenticated).toBe(false);

      await act(async () => {
        await result.current.login({ username: 'user', password: 'pass' });
      });

      expect(result.current.isAuthenticated).toBe(true);
    });

    it('should store accountId on successful login', async () => {
      const mockLogin = jest.fn().mockResolvedValue({
        token: 'test-token',
        accountId: 'account-123',
        accountName: 'Test Account',
      });
      (apiClient.login as jest.Mock) = mockLogin;

      const wrapper = ({ children }: { children: React.ReactNode }) => (
        <AuthProvider>{children}</AuthProvider>
      );
      const { result } = renderHook(() => useAuth(), { wrapper });

      await act(async () => {
        await result.current.login({ username: 'user', password: 'pass' });
      });

      expect(result.current.accountId).toBe('account-123');
    });

    it('should store accountName on successful login', async () => {
      const mockLogin = jest.fn().mockResolvedValue({
        token: 'test-token',
        accountId: 'account-123',
        accountName: 'Test Account',
      });
      (apiClient.login as jest.Mock) = mockLogin;

      const wrapper = ({ children }: { children: React.ReactNode }) => (
        <AuthProvider>{children}</AuthProvider>
      );
      const { result } = renderHook(() => useAuth(), { wrapper });

      await act(async () => {
        await result.current.login({ username: 'user', password: 'pass' });
      });

      expect(result.current.accountName).toBe('Test Account');
    });

    it('should set error on failed login', async () => {
      const mockLogin = jest.fn().mockRejectedValue({
        message: 'Invalid credentials',
        status: 401,
      });
      (apiClient.login as jest.Mock) = mockLogin;

      const wrapper = ({ children }: { children: React.ReactNode }) => (
        <AuthProvider>{children}</AuthProvider>
      );
      const { result } = renderHook(() => useAuth(), { wrapper });

      await act(async () => {
        try {
          await result.current.login({ username: 'user', password: 'wrong' });
        } catch {
          // Expected to throw
        }
      });

      expect(result.current.error).toBe('Invalid credentials');
    });
  });

  describe('logout', () => {
    it('should clear token from localStorage', async () => {
      localStorage.setItem('authToken', 'test-token');

      const wrapper = ({ children }: { children: React.ReactNode }) => (
        <AuthProvider>{children}</AuthProvider>
      );
      const { result } = renderHook(() => useAuth(), { wrapper });

      act(() => {
        result.current.logout();
      });

      expect(localStorage.getItem('authToken')).toBeNull();
    });

    it('should set isAuthenticated to false', async () => {
      localStorage.setItem('authToken', 'test-token');

      const wrapper = ({ children }: { children: React.ReactNode }) => (
        <AuthProvider>{children}</AuthProvider>
      );
      const { result } = renderHook(() => useAuth(), { wrapper });

      // Initially authenticated because token is in localStorage
      expect(result.current.isAuthenticated).toBe(true);

      act(() => {
        result.current.logout();
      });

      expect(result.current.isAuthenticated).toBe(false);
    });

    it('should clear accountId', async () => {
      localStorage.setItem('authToken', 'test-token');

      const wrapper = ({ children }: { children: React.ReactNode }) => (
        <AuthProvider>{children}</AuthProvider>
      );
      const { result } = renderHook(() => useAuth(), { wrapper });

      act(() => {
        result.current.logout();
      });

      expect(result.current.accountId).toBeNull();
    });
  });

  describe('session persistence', () => {
    it('should restore session from localStorage on mount', () => {
      localStorage.setItem('authToken', 'test-token');
      localStorage.setItem('accountId', 'account-123');
      localStorage.setItem('accountName', 'Test Account');

      const wrapper = ({ children }: { children: React.ReactNode }) => (
        <AuthProvider>{children}</AuthProvider>
      );
      const { result } = renderHook(() => useAuth(), { wrapper });

      expect(result.current.isAuthenticated).toBe(true);
      expect(result.current.accountId).toBe('account-123');
      expect(result.current.accountName).toBe('Test Account');
    });

    it('should not be authenticated if no token in localStorage', () => {
      const wrapper = ({ children }: { children: React.ReactNode }) => (
        <AuthProvider>{children}</AuthProvider>
      );
      const { result } = renderHook(() => useAuth(), { wrapper });

      expect(result.current.isAuthenticated).toBe(false);
    });
  });

  describe('clearError', () => {
    it('should clear error message', async () => {
      const mockLogin = jest.fn().mockRejectedValue({
        message: 'Invalid credentials',
        status: 401,
      });
      (apiClient.login as jest.Mock) = mockLogin;

      const wrapper = ({ children }: { children: React.ReactNode }) => (
        <AuthProvider>{children}</AuthProvider>
      );
      const { result } = renderHook(() => useAuth(), { wrapper });

      await act(async () => {
        try {
          await result.current.login({ username: 'user', password: 'wrong' });
        } catch {
          // Expected to throw
        }
      });

      expect(result.current.error).toBe('Invalid credentials');

      act(() => {
        result.current.clearError();
      });

      expect(result.current.error).toBeNull();
    });
  });
});
