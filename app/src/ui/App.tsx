import React from 'react';
import { AuthProvider } from './contexts/AuthContext';
import { TransactionProvider } from './contexts/TransactionContext';
import { Dashboard } from './components/Dashboard';
import './App.css';

const App: React.FC = () => {
  return (
    <AuthProvider>
      <TransactionProvider>
        <Dashboard />
      </TransactionProvider>
    </AuthProvider>
  );
};

export default App;
