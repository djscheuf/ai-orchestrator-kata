import { renderHook, act, waitFor } from '@testing-library/react';
import { TransactionProvider, useTransactions } from '../contexts/TransactionContext';
import { useAuth } from '../contexts/AuthContext';
import { apiClient } from '../api-client';

jest.mock('../api-client');
jest.mock('../contexts/AuthContext');

describe('TransactionContext', () => {
  const mockUseAuth = useAuth as jest.MockedFunction<typeof useAuth>;

  beforeEach(() => {
    jest.clearAllMocks();
    mockUseAuth.mockReturnValue({
      isAuthenticated: true,
      accountId: 'account-123',
      accountName: 'Test Account',
      error: null,
      login: jest.fn(),
      logout: jest.fn(),
      clearError: jest.fn(),
    });
  });

  describe('fetchTransactions', () => {
    it('should fetch transactions from API', async () => {
      const mockTransactions = [
        {
          id: 'tx-1',
          sourceAccountId: 'account-123',
          targetAccountId: 'account-456',
          amount: 100,
          date: '2026-01-15',
          createdAt: '2026-01-15T10:00:00Z',
        },
      ];

      const mockGetTransactions = jest.fn().mockResolvedValue(mockTransactions);
      (apiClient.getTransactions as jest.Mock) = mockGetTransactions;

      const wrapper = ({ children }: { children: React.ReactNode }) => (
        <TransactionProvider>{children}</TransactionProvider>
      );
      const { result } = renderHook(() => useTransactions(), { wrapper });

      await waitFor(() => {
        expect(result.current.transactions).toEqual(mockTransactions);
      });

      expect(mockGetTransactions).toHaveBeenCalled();
    });

    it('should set loading state during fetch', async () => {
      const mockGetTransactions = jest.fn(
        () =>
          new Promise((resolve) =>
            setTimeout(
              () =>
                resolve([
                  {
                    id: 'tx-1',
                    sourceAccountId: 'account-123',
                    targetAccountId: 'account-456',
                    amount: 100,
                    date: '2026-01-15',
                    createdAt: '2026-01-15T10:00:00Z',
                  },
                ]),
              10
            )
          )
      );
      (apiClient.getTransactions as jest.Mock) = mockGetTransactions;

      const wrapper = ({ children }: { children: React.ReactNode }) => (
        <TransactionProvider>{children}</TransactionProvider>
      );
      const { result } = renderHook(() => useTransactions(), { wrapper });

      expect(result.current.isLoading).toBe(true);

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });
    });

    it('should set error state on fetch failure', async () => {
      const mockGetTransactions = jest.fn().mockRejectedValue({
        message: 'Failed to fetch transactions',
        status: 500,
      });
      (apiClient.getTransactions as jest.Mock) = mockGetTransactions;

      const wrapper = ({ children }: { children: React.ReactNode }) => (
        <TransactionProvider>{children}</TransactionProvider>
      );
      const { result } = renderHook(() => useTransactions(), { wrapper });

      await waitFor(() => {
        expect(result.current.error).toBe('Failed to fetch transactions');
      });
    });

    it('should handle empty transaction list', async () => {
      const mockGetTransactions = jest.fn().mockResolvedValue([]);
      (apiClient.getTransactions as jest.Mock) = mockGetTransactions;

      const wrapper = ({ children }: { children: React.ReactNode }) => (
        <TransactionProvider>{children}</TransactionProvider>
      );
      const { result } = renderHook(() => useTransactions(), { wrapper });

      await waitFor(() => {
        expect(result.current.transactions).toEqual([]);
      });
    });
  });

  describe('balance calculation', () => {
    it('should calculate balance from transactions', async () => {
      const mockTransactions = [
        {
          id: 'tx-1',
          sourceAccountId: 'account-123',
          targetAccountId: 'account-456',
          amount: 100,
          date: '2026-01-15',
          createdAt: '2026-01-15T10:00:00Z',
        },
        {
          id: 'tx-2',
          sourceAccountId: 'account-456',
          targetAccountId: 'account-123',
          amount: 50,
          date: '2026-01-16',
          createdAt: '2026-01-16T10:00:00Z',
        },
      ];

      const mockGetTransactions = jest.fn().mockResolvedValue(mockTransactions);
      (apiClient.getTransactions as jest.Mock) = mockGetTransactions;

      const wrapper = ({ children }: { children: React.ReactNode }) => (
        <TransactionProvider>{children}</TransactionProvider>
      );
      const { result } = renderHook(() => useTransactions(), { wrapper });

      await waitFor(() => {
        expect(result.current.balance).toBe(-50);
      });
    });

    it('should calculate balance as incoming minus outgoing', async () => {
      const mockTransactions = [
        {
          id: 'tx-1',
          sourceAccountId: 'account-123',
          targetAccountId: 'account-456',
          amount: 100,
          date: '2026-01-15',
          createdAt: '2026-01-15T10:00:00Z',
        },
        {
          id: 'tx-2',
          sourceAccountId: 'account-456',
          targetAccountId: 'account-123',
          amount: 75,
          date: '2026-01-16',
          createdAt: '2026-01-16T10:00:00Z',
        },
      ];

      const mockGetTransactions = jest.fn().mockResolvedValue(mockTransactions);
      (apiClient.getTransactions as jest.Mock) = mockGetTransactions;

      const wrapper = ({ children }: { children: React.ReactNode }) => (
        <TransactionProvider>{children}</TransactionProvider>
      );
      const { result } = renderHook(() => useTransactions(), { wrapper });

      await waitFor(() => {
        expect(result.current.balance).toBe(-25);
      });
    });

    it('should return zero balance for empty transaction list', async () => {
      const mockGetTransactions = jest.fn().mockResolvedValue([]);
      (apiClient.getTransactions as jest.Mock) = mockGetTransactions;

      const wrapper = ({ children }: { children: React.ReactNode }) => (
        <TransactionProvider>{children}</TransactionProvider>
      );
      const { result } = renderHook(() => useTransactions(), { wrapper });

      await waitFor(() => {
        expect(result.current.balance).toBe(0);
      });
    });
  });

  describe('addTransaction', () => {
    it('should call apiClient.createTransaction with request', async () => {
      const mockGetTransactions = jest.fn().mockResolvedValue([]);
      const mockCreateTransaction = jest.fn().mockResolvedValue({
        id: 'tx-1',
        sourceAccountId: 'account-123',
        targetAccountId: 'account-456',
        amount: 100,
        date: '2026-01-15',
        createdAt: '2026-01-15T10:00:00Z',
      });

      (apiClient.getTransactions as jest.Mock) = mockGetTransactions;
      (apiClient.createTransaction as jest.Mock) = mockCreateTransaction;

      const wrapper = ({ children }: { children: React.ReactNode }) => (
        <TransactionProvider>{children}</TransactionProvider>
      );
      const { result } = renderHook(() => useTransactions(), { wrapper });

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      await act(async () => {
        await result.current.addTransaction({
          targetAccountId: 'account-456',
          amount: 100,
          date: '2026-01-15',
        });
      });

      expect(mockCreateTransaction).toHaveBeenCalledWith({
        targetAccountId: 'account-456',
        amount: 100,
        date: '2026-01-15',
      });
    });

    it('should refresh transactions after adding', async () => {
      const mockGetTransactions = jest.fn()
        .mockResolvedValueOnce([])
        .mockResolvedValueOnce([
          {
            id: 'tx-1',
            sourceAccountId: 'account-123',
            targetAccountId: 'account-456',
            amount: 100,
            date: '2026-01-15',
            createdAt: '2026-01-15T10:00:00Z',
          },
        ]);

      const mockCreateTransaction = jest.fn().mockResolvedValue({
        id: 'tx-1',
        sourceAccountId: 'account-123',
        targetAccountId: 'account-456',
        amount: 100,
        date: '2026-01-15',
        createdAt: '2026-01-15T10:00:00Z',
      });

      (apiClient.getTransactions as jest.Mock) = mockGetTransactions;
      (apiClient.createTransaction as jest.Mock) = mockCreateTransaction;

      const wrapper = ({ children }: { children: React.ReactNode }) => (
        <TransactionProvider>{children}</TransactionProvider>
      );
      const { result } = renderHook(() => useTransactions(), { wrapper });

      await waitFor(() => {
        expect(result.current.transactions).toEqual([]);
      });

      await act(async () => {
        await result.current.addTransaction({
          targetAccountId: 'account-456',
          amount: 100,
          date: '2026-01-15',
        });
      });

      await waitFor(() => {
        expect(result.current.transactions).toHaveLength(1);
      });
    });

    it('should update balance after adding transaction', async () => {
      const mockGetTransactions = jest.fn()
        .mockResolvedValueOnce([])
        .mockResolvedValueOnce([
          {
            id: 'tx-1',
            sourceAccountId: 'account-123',
            targetAccountId: 'account-456',
            amount: 100,
            date: '2026-01-15',
            createdAt: '2026-01-15T10:00:00Z',
          },
        ]);

      const mockCreateTransaction = jest.fn().mockResolvedValue({
        id: 'tx-1',
        sourceAccountId: 'account-123',
        targetAccountId: 'account-456',
        amount: 100,
        date: '2026-01-15',
        createdAt: '2026-01-15T10:00:00Z',
      });

      (apiClient.getTransactions as jest.Mock) = mockGetTransactions;
      (apiClient.createTransaction as jest.Mock) = mockCreateTransaction;

      const wrapper = ({ children }: { children: React.ReactNode }) => (
        <TransactionProvider>{children}</TransactionProvider>
      );
      const { result } = renderHook(() => useTransactions(), { wrapper });

      await waitFor(() => {
        expect(result.current.balance).toBe(0);
      });

      await act(async () => {
        await result.current.addTransaction({
          targetAccountId: 'account-456',
          amount: 100,
          date: '2026-01-15',
        });
      });

      await waitFor(() => {
        expect(result.current.balance).toBe(-100);
      });
    });

    it('should set error on failed transaction creation', async () => {
      const mockGetTransactions = jest.fn().mockResolvedValue([]);
      const mockCreateTransaction = jest.fn().mockRejectedValue({
        message: 'Insufficient funds',
        status: 422,
      });

      (apiClient.getTransactions as jest.Mock) = mockGetTransactions;
      (apiClient.createTransaction as jest.Mock) = mockCreateTransaction;

      const wrapper = ({ children }: { children: React.ReactNode }) => (
        <TransactionProvider>{children}</TransactionProvider>
      );
      const { result } = renderHook(() => useTransactions(), { wrapper });

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      await act(async () => {
        try {
          await result.current.addTransaction({
            targetAccountId: 'account-456',
            amount: 1000,
            date: '2026-01-15',
          });
        } catch {
          // Expected to throw
        }
      });

      expect(result.current.error).toBe('Insufficient funds');
    });
  });

  describe('clearError', () => {
    it('should clear error message', async () => {
      const mockGetTransactions = jest.fn().mockRejectedValue({
        message: 'Failed to fetch',
        status: 500,
      });

      (apiClient.getTransactions as jest.Mock) = mockGetTransactions;

      const wrapper = ({ children }: { children: React.ReactNode }) => (
        <TransactionProvider>{children}</TransactionProvider>
      );
      const { result } = renderHook(() => useTransactions(), { wrapper });

      await waitFor(() => {
        expect(result.current.error).toBe('Failed to fetch');
      });

      act(() => {
        result.current.clearError();
      });

      expect(result.current.error).toBeNull();
    });
  });
});
