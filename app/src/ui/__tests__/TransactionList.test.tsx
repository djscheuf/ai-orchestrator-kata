import React from 'react';
import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { TransactionList } from '../components/TransactionList';
import { Transaction } from '../types';

describe('TransactionList', () => {
  const mockTransactions: Transaction[] = [
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

  it('should render transaction table', () => {
    render(<TransactionList transactions={mockTransactions} />);
    expect(screen.getByRole('table')).toBeInTheDocument();
  });

  it('should display all transactions', () => {
    render(<TransactionList transactions={mockTransactions} />);
    expect(screen.getByText('tx-1')).toBeInTheDocument();
    expect(screen.getByText('tx-2')).toBeInTheDocument();
  });

  it('should display transaction amounts', () => {
    render(<TransactionList transactions={mockTransactions} />);
    expect(screen.getByText(/\$100\.00/)).toBeInTheDocument();
    expect(screen.getByText(/\$50\.00/)).toBeInTheDocument();
  });

  it('should display transaction dates', () => {
    render(<TransactionList transactions={mockTransactions} />);
    expect(screen.getByText(/Jan 15, 2026/)).toBeInTheDocument();
    expect(screen.getByText(/Jan 16, 2026/)).toBeInTheDocument();
  });

  it('should display source and target account IDs', () => {
    render(<TransactionList transactions={mockTransactions} />);
    expect(screen.getByText('account-123')).toBeInTheDocument();
    expect(screen.getByText('account-456')).toBeInTheDocument();
  });

  it('should display empty state when no transactions', () => {
    render(<TransactionList transactions={[]} />);
    expect(screen.getByText('No transactions yet')).toBeInTheDocument();
  });

  it('should display table headers', () => {
    render(<TransactionList transactions={mockTransactions} />);
    expect(screen.getByText('Date')).toBeInTheDocument();
    expect(screen.getByText('From')).toBeInTheDocument();
    expect(screen.getByText('To')).toBeInTheDocument();
    expect(screen.getByText('Amount')).toBeInTheDocument();
  });

  it('should sort transactions by date descending by default', () => {
    render(<TransactionList transactions={mockTransactions} />);
    const rows = screen.getAllByRole('row');
    const firstDataRow = within(rows[1]).getByText('tx-2');
    const secondDataRow = within(rows[2]).getByText('tx-1');
    expect(firstDataRow).toBeInTheDocument();
    expect(secondDataRow).toBeInTheDocument();
  });

  it('should format currency amounts correctly', () => {
    render(<TransactionList transactions={mockTransactions} />);
    expect(screen.getByText('$100.00')).toBeInTheDocument();
    expect(screen.getByText('$50.00')).toBeInTheDocument();
  });

  it('should format dates using en-US locale', () => {
    render(<TransactionList transactions={mockTransactions} />);
    expect(screen.getByText(/Jan 15, 2026/)).toBeInTheDocument();
    expect(screen.getByText(/Jan 16, 2026/)).toBeInTheDocument();
  });

  it('should handle loading state', () => {
    render(<TransactionList transactions={mockTransactions} isLoading={true} />);
    expect(screen.getByText('Loading transactions...')).toBeInTheDocument();
  });

  it('should display transaction ID in table', () => {
    render(<TransactionList transactions={mockTransactions} />);
    expect(screen.getByText('tx-1')).toBeInTheDocument();
    expect(screen.getByText('tx-2')).toBeInTheDocument();
  });
});
