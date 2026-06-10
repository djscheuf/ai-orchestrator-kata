import React, { useState } from 'react';
import { CreateTransactionRequest } from '../types';

interface TransactionFormProps {
  onSubmit: (request: CreateTransactionRequest) => Promise<void>;
}

interface FormErrors {
  date?: string;
  amount?: string;
  targetAccountId?: string;
}

export const TransactionForm: React.FC<TransactionFormProps> = ({
  onSubmit,
}) => {
  const [date, setDate] = useState('');
  const [amount, setAmount] = useState('');
  const [targetAccountId, setTargetAccountId] = useState('');
  const [errors, setErrors] = useState<FormErrors>({});
  const [isLoading, setIsLoading] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);

  const validateForm = (): boolean => {
    const newErrors: FormErrors = {};

    if (!date) {
      newErrors.date = 'Date is required';
    } else {
      const selectedDate = new Date(date);
      const today = new Date();
      today.setHours(0, 0, 0, 0);
      if (selectedDate > today) {
        newErrors.date = 'Future dates not allowed';
      }
    }

    if (!amount) {
      newErrors.amount = 'Amount is required';
    } else {
      const amountNum = parseFloat(amount);
      if (amountNum <= 0) {
        newErrors.amount = 'Amount must be non-zero and positive';
      }
    }

    if (!targetAccountId) {
      newErrors.targetAccountId = 'Target account is required';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitError(null);

    if (!validateForm()) {
      return;
    }

    setIsLoading(true);
    try {
      await onSubmit({
        date,
        amount: parseFloat(amount),
        targetAccountId,
      });

      setDate('');
      setAmount('');
      setTargetAccountId('');
      setErrors({});
    } catch (err: any) {
      setSubmitError(err.message || 'Failed to create transaction');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="transaction-form">
      <div className="form-group">
        <label htmlFor="date">Date</label>
        <input
          id="date"
          type="date"
          value={date}
          onChange={(e) => setDate(e.target.value)}
          disabled={isLoading}
        />
        {errors.date && <span className="error">{errors.date}</span>}
      </div>

      <div className="form-group">
        <label htmlFor="amount">Amount</label>
        <input
          id="amount"
          type="number"
          step="0.01"
          value={amount}
          onChange={(e) => setAmount(e.target.value)}
          disabled={isLoading}
        />
        {errors.amount && <span className="error">{errors.amount}</span>}
      </div>

      <div className="form-group">
        <label htmlFor="targetAccountId">Target Account</label>
        <input
          id="targetAccountId"
          type="text"
          value={targetAccountId}
          onChange={(e) => setTargetAccountId(e.target.value)}
          disabled={isLoading}
        />
        {errors.targetAccountId && (
          <span className="error">{errors.targetAccountId}</span>
        )}
      </div>

      {submitError && <div className="error-message">{submitError}</div>}

      <button type="submit" disabled={isLoading}>
        {isLoading ? 'Loading...' : 'Submit'}
      </button>
    </form>
  );
};
