import React from 'react';

interface BalanceDisplayProps {
  balance: number;
  accountName: string;
}

export const BalanceDisplay: React.FC<BalanceDisplayProps> = ({
  balance,
  accountName,
}) => {
  const formattedBalance = new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(balance);

  return (
    <div className="balance-display">
      <h2>{accountName}</h2>
      <div className="balance-amount">{formattedBalance}</div>
    </div>
  );
};
