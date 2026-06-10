import React from 'react';
import { useAuth } from '../contexts/AuthContext';
import { useTransactions } from '../contexts/TransactionContext';
import { LoginForm } from './LoginForm';
import { TransactionList } from './TransactionList';
import { BalanceDisplay } from './BalanceDisplay';
import { TransactionForm } from './TransactionForm';

export const Dashboard: React.FC = () => {
  const auth = useAuth();
  const transactions = useTransactions();

  if (!auth.isAuthenticated) {
    return (
      <div className="dashboard login-view">
        <h1>Financial Dashboard</h1>
        <LoginForm onLogin={auth.login} />
      </div>
    );
  }

  return (
    <div className="dashboard transaction-view">
      <div className="header">
        <h1>Financial Dashboard</h1>
        <button onClick={auth.logout} className="logout-button">
          Logout
        </button>
      </div>

      <div className="content">
        <div className="balance-section">
          {auth.accountName && (
            <BalanceDisplay
              balance={transactions.balance}
              accountName={auth.accountName}
            />
          )}
        </div>

        <div className="transaction-section">
          <h2>Transactions</h2>
          <TransactionList
            transactions={transactions.transactions}
            isLoading={transactions.isLoading}
          />
        </div>

        <div className="form-section">
          <h2>Add Transaction</h2>
          <TransactionForm onSubmit={transactions.addTransaction} />
          {transactions.error && (
            <div className="error-message">{transactions.error}</div>
          )}
        </div>
      </div>
    </div>
  );
};
