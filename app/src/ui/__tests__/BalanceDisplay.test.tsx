import React from 'react';
import { render, screen } from '@testing-library/react';
import { BalanceDisplay } from '../components/BalanceDisplay';

describe('BalanceDisplay', () => {
  it('should render balance value', () => {
    render(<BalanceDisplay balance={1000} accountName="Test Account" />);
    expect(screen.getByText(/1000/)).toBeInTheDocument();
  });

  it('should format balance as currency with 2 decimal places', () => {
    render(<BalanceDisplay balance={1234.56} accountName="Test Account" />);
    expect(screen.getByText(/1,234\.56/)).toBeInTheDocument();
  });

  it('should format negative balance as currency', () => {
    render(<BalanceDisplay balance={-500.25} accountName="Test Account" />);
    expect(screen.getByText(/-500\.25/)).toBeInTheDocument();
  });

  it('should display account name', () => {
    render(<BalanceDisplay balance={1000} accountName="My Checking Account" />);
    expect(screen.getByText('My Checking Account')).toBeInTheDocument();
  });

  it('should display zero balance correctly', () => {
    render(<BalanceDisplay balance={0} accountName="Test Account" />);
    expect(screen.getByText(/0\.00/)).toBeInTheDocument();
  });

  it('should format large balance with thousands separator', () => {
    render(<BalanceDisplay balance={1000000.99} accountName="Test Account" />);
    expect(screen.getByText(/1,000,000\.99/)).toBeInTheDocument();
  });

  it('should display balance with dollar sign', () => {
    render(<BalanceDisplay balance={100} accountName="Test Account" />);
    const balanceText = screen.getByText(/\$/).textContent;
    expect(balanceText).toContain('$');
  });
});
