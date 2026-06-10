import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { TransactionForm } from '../components/TransactionForm';

describe('TransactionForm', () => {
  const mockOnSubmit = jest.fn();

  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('should render form with date input', () => {
    render(<TransactionForm onSubmit={mockOnSubmit} />);
    expect(screen.getByLabelText(/date/i)).toBeInTheDocument();
  });

  it('should render form with amount input', () => {
    render(<TransactionForm onSubmit={mockOnSubmit} />);
    expect(screen.getByLabelText(/amount/i)).toBeInTheDocument();
  });

  it('should render form with target account input', () => {
    render(<TransactionForm onSubmit={mockOnSubmit} />);
    expect(screen.getByLabelText(/target account/i)).toBeInTheDocument();
  });

  it('should render submit button', () => {
    render(<TransactionForm onSubmit={mockOnSubmit} />);
    expect(screen.getByRole('button', { name: /submit/i })).toBeInTheDocument();
  });

  it('should validate required fields', async () => {
    const user = userEvent.setup();
    render(<TransactionForm onSubmit={mockOnSubmit} />);

    const submitButton = screen.getByRole('button', { name: /submit/i });
    await user.click(submitButton);

    expect(screen.getByText(/date is required/i)).toBeInTheDocument();
    expect(screen.getByText(/amount is required/i)).toBeInTheDocument();
    expect(screen.getByText(/target account is required/i)).toBeInTheDocument();
  });

  it('should validate date is not in future', async () => {
    const user = userEvent.setup();
    render(<TransactionForm onSubmit={mockOnSubmit} />);

    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    const dateString = tomorrow.toISOString().split('T')[0];

    const dateInput = screen.getByLabelText(/date/i);
    await user.type(dateInput, dateString);

    const submitButton = screen.getByRole('button', { name: /submit/i });
    await user.click(submitButton);

    expect(screen.getByText(/future dates not allowed/i)).toBeInTheDocument();
  });

  it('should validate amount is positive', async () => {
    const user = userEvent.setup();
    render(<TransactionForm onSubmit={mockOnSubmit} />);

    const amountInput = screen.getByLabelText(/amount/i);
    await user.type(amountInput, '-100');

    const submitButton = screen.getByRole('button', { name: /submit/i });
    await user.click(submitButton);

    expect(screen.getByText(/amount must be positive/i)).toBeInTheDocument();
  });

  it('should validate amount is not zero', async () => {
    const user = userEvent.setup();
    render(<TransactionForm onSubmit={mockOnSubmit} />);

    const amountInput = screen.getByLabelText(/amount/i);
    await user.type(amountInput, '0');

    const submitButton = screen.getByRole('button', { name: /submit/i });
    await user.click(submitButton);

    expect(screen.getByText(/amount must be non-zero/i)).toBeInTheDocument();
  });

  it('should submit valid form', async () => {
    const user = userEvent.setup();
    render(<TransactionForm onSubmit={mockOnSubmit} />);

    const today = new Date().toISOString().split('T')[0];

    const dateInput = screen.getByLabelText(/date/i);
    await user.type(dateInput, today);

    const amountInput = screen.getByLabelText(/amount/i);
    await user.type(amountInput, '100');

    const targetAccountInput = screen.getByLabelText(/target account/i);
    await user.type(targetAccountInput, 'account-456');

    const submitButton = screen.getByRole('button', { name: /submit/i });
    await user.click(submitButton);

    await waitFor(() => {
      expect(mockOnSubmit).toHaveBeenCalledWith({
        date: today,
        amount: 100,
        targetAccountId: 'account-456',
      });
    });
  });

  it('should clear form after successful submission', async () => {
    const user = userEvent.setup();
    render(<TransactionForm onSubmit={mockOnSubmit} />);

    const today = new Date().toISOString().split('T')[0];

    const dateInput = screen.getByLabelText(/date/i) as HTMLInputElement;
    await user.type(dateInput, today);

    const amountInput = screen.getByLabelText(/amount/i) as HTMLInputElement;
    await user.type(amountInput, '100');

    const targetAccountInput = screen.getByLabelText(
      /target account/i
    ) as HTMLInputElement;
    await user.type(targetAccountInput, 'account-456');

    const submitButton = screen.getByRole('button', { name: /submit/i });
    await user.click(submitButton);

    await waitFor(() => {
      expect(dateInput.value).toBe('');
      expect(amountInput.value).toBe('');
      expect(targetAccountInput.value).toBe('');
    });
  });

  it('should display error message on submission failure', async () => {
    const user = userEvent.setup();
    const mockError = new Error('Insufficient funds');
    mockOnSubmit.mockRejectedValue(mockError);

    render(<TransactionForm onSubmit={mockOnSubmit} />);

    const today = new Date().toISOString().split('T')[0];

    const dateInput = screen.getByLabelText(/date/i);
    await user.type(dateInput, today);

    const amountInput = screen.getByLabelText(/amount/i);
    await user.type(amountInput, '100');

    const targetAccountInput = screen.getByLabelText(/target account/i);
    await user.type(targetAccountInput, 'account-456');

    const submitButton = screen.getByRole('button', { name: /submit/i });
    await user.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText(/insufficient funds/i)).toBeInTheDocument();
    });
  });

  it('should show loading state during submission', async () => {
    const user = userEvent.setup();
    mockOnSubmit.mockImplementation(
      () =>
        new Promise((resolve) => setTimeout(resolve, 100))
    );

    render(<TransactionForm onSubmit={mockOnSubmit} />);

    const today = new Date().toISOString().split('T')[0];

    const dateInput = screen.getByLabelText(/date/i);
    await user.type(dateInput, today);

    const amountInput = screen.getByLabelText(/amount/i);
    await user.type(amountInput, '100');

    const targetAccountInput = screen.getByLabelText(/target account/i);
    await user.type(targetAccountInput, 'account-456');

    const submitButton = screen.getByRole('button', { name: /submit/i });
    await user.click(submitButton);

    expect(screen.getByRole('button', { name: /loading/i })).toBeInTheDocument();
  });
});
