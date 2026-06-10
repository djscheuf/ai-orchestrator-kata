import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { Dashboard } from '../components/Dashboard';
import { AuthProvider } from '../contexts/AuthContext';
import { TransactionProvider } from '../contexts/TransactionContext';

jest.mock('../api-client');

const renderDashboard = () => {
  return render(
    <AuthProvider>
      <TransactionProvider>
        <Dashboard />
      </TransactionProvider>
    </AuthProvider>
  );
};

describe('Dashboard', () => {
  beforeEach(() => {
    localStorage.clear();
    jest.clearAllMocks();
  });

  it('should render login view when not authenticated', () => {
    renderDashboard();
    expect(screen.getByLabelText(/username/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/password/i)).toBeInTheDocument();
  });

  it('should render transaction view when authenticated', async () => {
    localStorage.setItem('authToken', 'test-token');
    localStorage.setItem('accountId', 'account-123');
    localStorage.setItem('accountName', 'Test Account');

    renderDashboard();

    await waitFor(() => {
      expect(screen.queryByLabelText(/username/i)).not.toBeInTheDocument();
    });
  });

  it('should display logout button when authenticated', async () => {
    localStorage.setItem('authToken', 'test-token');
    localStorage.setItem('accountId', 'account-123');
    localStorage.setItem('accountName', 'Test Account');

    renderDashboard();

    await waitFor(() => {
      expect(screen.getByRole('button', { name: /logout/i })).toBeInTheDocument();
    });
  });

  it('should display transaction list when authenticated', async () => {
    localStorage.setItem('authToken', 'test-token');
    localStorage.setItem('accountId', 'account-123');
    localStorage.setItem('accountName', 'Test Account');

    renderDashboard();

    await waitFor(() => {
      expect(screen.getByRole('table')).toBeInTheDocument();
    });
  });

  it('should display balance when authenticated', async () => {
    localStorage.setItem('authToken', 'test-token');
    localStorage.setItem('accountId', 'account-123');
    localStorage.setItem('accountName', 'Test Account');

    renderDashboard();

    await waitFor(() => {
      expect(screen.getByText('Test Account')).toBeInTheDocument();
    });
  });

  it('should display transaction form when authenticated', async () => {
    localStorage.setItem('authToken', 'test-token');
    localStorage.setItem('accountId', 'account-123');
    localStorage.setItem('accountName', 'Test Account');

    renderDashboard();

    await waitFor(() => {
      expect(screen.getByLabelText(/date/i)).toBeInTheDocument();
      expect(screen.getByLabelText(/amount/i)).toBeInTheDocument();
      expect(screen.getByLabelText(/target account/i)).toBeInTheDocument();
    });
  });

  it('should redirect to login on logout', async () => {
    const user = userEvent.setup();
    localStorage.setItem('authToken', 'test-token');
    localStorage.setItem('accountId', 'account-123');
    localStorage.setItem('accountName', 'Test Account');

    renderDashboard();

    await waitFor(() => {
      expect(screen.getByRole('button', { name: /logout/i })).toBeInTheDocument();
    });

    const logoutButton = screen.getByRole('button', { name: /logout/i });
    await user.click(logoutButton);

    await waitFor(() => {
      expect(screen.getByLabelText(/username/i)).toBeInTheDocument();
    });
  });

  it('should display title', () => {
    renderDashboard();
    expect(screen.getByText(/financial dashboard/i)).toBeInTheDocument();
  });
});
