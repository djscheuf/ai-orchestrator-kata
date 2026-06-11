/**
 * API Client for the financial application.
 *
 * Provides type-safe methods for interacting with the backend API.
 * Handles authentication token management and error handling.
 */

import {
  LoginRequest,
  LoginResponse,
  Transaction,
  CreateTransactionRequest,
  Balance,
  ApiError,
} from './types';

let API_BASE_URL = 'http://localhost:5000/api';

// Load API base URL from config file
async function loadConfig() {
  try {
    const response = await fetch('/config.json');
    if (response.ok) {
      const config = await response.json();
      if (config.apiBaseUrl) {
        API_BASE_URL = `${config.apiBaseUrl}/api`;
      }
    }
  } catch (error) {
    console.warn('Failed to load config.json, using default API URL', error);
  }
}

loadConfig();

/**
 * Configuration for API client.
 */
interface ApiClientConfig {
  baseUrl?: string;
  timeout?: number;
}

class ApiClient {
  private token: string | null = null;

  /**
   * Set the authentication token.
   * Called after successful login.
   */
  setToken(token: string): void {
    this.token = token;
    localStorage.setItem('authToken', token);
  }

  /**
   * Get the stored authentication token.
   */
  getToken(): string | null {
    if (!this.token) {
      this.token = localStorage.getItem('authToken');
    }
    return this.token;
  }

  /**
   * Clear the authentication token.
   * Called on logout.
   */
  clearToken(): void {
    this.token = null;
    localStorage.removeItem('authToken');
  }

  /**
   * Build headers for API requests.
   * Includes Authorization header if token is available.
   */
  private getHeaders(includeAuth: boolean = true): HeadersInit {
    const headers: HeadersInit = {
      'Content-Type': 'application/json',
    };

    if (includeAuth && this.getToken()) {
      headers['Authorization'] = `Bearer ${this.getToken()}`;
    }

    return headers;
  }

  /**
   * Handle API response and throw error if not successful.
   */
  private async handleResponse<T>(response: Response): Promise<T> {
    if (!response.ok) {
      const error: ApiError = {
        message: `API Error: ${response.statusText}`,
        status: response.status,
      };

      try {
        const errorData = await response.json();
        error.message = errorData.message || error.message;
      } catch {
        // Use default error message if response is not JSON
      }

      throw error;
    }

    return response.json() as Promise<T>;
  }

  /**
   * Authenticate user with username and password.
   * Returns token and account information.
   */
  async login(request: LoginRequest): Promise<LoginResponse> {
    const response = await fetch(`${API_BASE_URL}/auth/login`, {
      method: 'POST',
      headers: this.getHeaders(false),
      body: JSON.stringify(request),
    });

    const data = await this.handleResponse<LoginResponse>(response);
    this.setToken(data.token);
    return data;
  }

  /**
   * Get all transactions for the authenticated account.
   * Requires valid authentication token.
   */
  async getTransactions(accountId: string): Promise<Transaction[]> {
    const response = await fetch(`${API_BASE_URL}/accounts/${accountId}/transactions`, {
      method: 'GET',
      headers: this.getHeaders(true),
    });

    const data = await this.handleResponse<{ transactions: Transaction[]; balance: number }>(response);
    return data.transactions;
  }

  /**
   * Create a new transaction from the authenticated account.
   * Requires valid authentication token.
   */
  async createTransaction(
    accountId: string,
    request: CreateTransactionRequest
  ): Promise<Transaction> {
    const response = await fetch(`${API_BASE_URL}/accounts/${accountId}/transactions`, {
      method: 'POST',
      headers: this.getHeaders(true),
      body: JSON.stringify(request),
    });

    return this.handleResponse<Transaction>(response);
  }

  /**
   * Get current balance for the authenticated account.
   * Requires valid authentication token.
   */
  async getBalance(accountId: string): Promise<Balance> {
    const response = await fetch(`${API_BASE_URL}/accounts/${accountId}/balance`, {
      method: 'GET',
      headers: this.getHeaders(true),
    });

    return this.handleResponse<Balance>(response);
  }
}

// Export singleton instance
export const apiClient = new ApiClient();

// Export class for testing
export default ApiClient;
