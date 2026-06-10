/**
 * Shared TypeScript type definitions for the financial application.
 */

export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  accountId: string;
  accountName: string;
}

export interface Transaction {
  id: string;
  sourceAccountId: string;
  targetAccountId: string;
  amount: number;
  date: string;
  createdAt?: string;
}

export interface CreateTransactionRequest {
  targetAccountId: string;
  amount: number;
  date: string;
}

export interface Balance {
  accountId: string;
  accountName: string;
  balance: number;
}

export interface ApiError {
  message: string;
  status: number;
}
