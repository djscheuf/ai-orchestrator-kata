import React, { createContext, useContext, useState, useEffect } from 'react';
import { Transaction, CreateTransactionRequest } from '../types';
import { apiClient } from '../api-client';
import { useAuth } from './AuthContext';

interface TransactionContextType {
  transactions: Transaction[];
  balance: number;
  isLoading: boolean;
  error: string | null;
  addTransaction: (request: CreateTransactionRequest) => Promise<void>;
  clearError: () => void;
  refreshTransactions: () => Promise<void>;
}

const TransactionContext = createContext<TransactionContextType | undefined>(
  undefined
);

export const TransactionProvider: React.FC<{ children: React.ReactNode }> = ({
  children,
}) => {
  const auth = useAuth();
  const [transactions, setTransactions] = useState<Transaction[]>([]);
  const [balance, setBalance] = useState(0);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const calculateBalance = (txs: Transaction[]): number => {
    let incoming = 0;
    let outgoing = 0;

    txs.forEach((tx) => {
      if (tx.targetAccountId === auth.accountId) {
        incoming += tx.amount;
      }
      if (tx.sourceAccountId === auth.accountId) {
        outgoing += tx.amount;
      }
    });

    return incoming - outgoing;
  };

  const refreshTransactions = async (): Promise<void> => {
    try {
      setError(null);
      const fetchedTransactions = await apiClient.getTransactions();
      setTransactions(fetchedTransactions);
      setBalance(calculateBalance(fetchedTransactions));
    } catch (err: any) {
      const errorMessage = err.message || 'Failed to fetch transactions';
      setError(errorMessage);
      throw err;
    }
  };

  useEffect(() => {
    if (auth.isAuthenticated) {
      setIsLoading(true);
      refreshTransactions().finally(() => setIsLoading(false));
    }
  }, [auth.isAuthenticated, auth.accountId]);

  const addTransaction = async (
    request: CreateTransactionRequest
  ): Promise<void> => {
    try {
      setError(null);
      await apiClient.createTransaction(request);
      await refreshTransactions();
    } catch (err: any) {
      const errorMessage = err.message || 'Failed to create transaction';
      setError(errorMessage);
      throw err;
    }
  };

  const clearError = (): void => {
    setError(null);
  };

  const value: TransactionContextType = {
    transactions,
    balance,
    isLoading,
    error,
    addTransaction,
    clearError,
    refreshTransactions,
  };

  return (
    <TransactionContext.Provider value={value}>
      {children}
    </TransactionContext.Provider>
  );
};

export const useTransactions = (): TransactionContextType => {
  const context = useContext(TransactionContext);
  if (context === undefined) {
    throw new Error(
      'useTransactions must be used within a TransactionProvider'
    );
  }
  return context;
};
