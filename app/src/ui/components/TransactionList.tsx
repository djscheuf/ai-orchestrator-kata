import React, { useMemo } from 'react';
import {
  useReactTable,
  getCoreRowModel,
  getSortedRowModel,
  flexRender,
  createColumnHelper,
  SortingState,
} from '@tanstack/react-table';
import { Transaction } from '../types';

interface TransactionListProps {
  transactions: Transaction[];
  isLoading?: boolean;
}

const columnHelper = createColumnHelper<Transaction>();

export const TransactionList: React.FC<TransactionListProps> = ({
  transactions,
  isLoading = false,
}) => {
  const [sorting, setSorting] = React.useState<SortingState>([
    { id: 'date', desc: true },
  ]);

  const columns = useMemo(
    () => [
      columnHelper.accessor('id', {
        header: 'ID',
        cell: (info) => info.getValue(),
      }),
      columnHelper.accessor('date', {
        header: 'Date',
        cell: (info) => {
          const date = new Date(info.getValue());
          return new Intl.DateTimeFormat('en-US', {
            year: 'numeric',
            month: 'short',
            day: 'numeric',
          }).format(date);
        },
      }),
      columnHelper.accessor('sourceAccountId', {
        header: 'From',
        cell: (info) => info.getValue(),
      }),
      columnHelper.accessor('targetAccountId', {
        header: 'To',
        cell: (info) => info.getValue(),
      }),
      columnHelper.accessor('amount', {
        header: 'Amount',
        cell: (info) => {
          const amount = info.getValue();
          return new Intl.NumberFormat('en-US', {
            style: 'currency',
            currency: 'USD',
            minimumFractionDigits: 2,
            maximumFractionDigits: 2,
          }).format(amount);
        },
      }),
    ],
    []
  );

  const table = useReactTable({
    data: transactions,
    columns,
    state: {
      sorting,
    },
    onSortingChange: setSorting,
    getCoreRowModel: getCoreRowModel(),
    getSortedRowModel: getSortedRowModel(),
  });

  if (isLoading) {
    return <div className="transaction-list-loading">Loading transactions...</div>;
  }

  if (transactions.length === 0) {
    return <div className="transaction-list-empty">No transactions yet</div>;
  }

  return (
    <div className="transaction-list">
      <table>
        <thead>
          {table.getHeaderGroups().map((headerGroup) => (
            <tr key={headerGroup.id}>
              {headerGroup.headers.map((header) => (
                <th
                  key={header.id}
                  onClick={header.column.getToggleSortingHandler()}
                  style={{ cursor: 'pointer' }}
                >
                  {flexRender(
                    header.column.columnDef.header,
                    header.getContext()
                  )}
                  {header.column.getIsSorted() === 'asc' && ' 🔼'}
                  {header.column.getIsSorted() === 'desc' && ' 🔽'}
                </th>
              ))}
            </tr>
          ))}
        </thead>
        <tbody>
          {table.getRowModel().rows.map((row) => (
            <tr key={row.id}>
              {row.getVisibleCells().map((cell) => (
                <td key={cell.id}>
                  {flexRender(cell.column.columnDef.cell, cell.getContext())}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};
