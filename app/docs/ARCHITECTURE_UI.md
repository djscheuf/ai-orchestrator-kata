# Frontend Architecture (React + TypeScript)

## Overview

The frontend uses React with TypeScript, leveraging stateful hooks and React Context for state management. Components are organized into pages and reusable components, with shadcn/ui providing the majority of UI elements and TanStack Table for transaction display.

---

## State Management Strategy

### Stateful Hooks & Context

**Philosophy:** Keep state management simple and co-located with components that use it.

**Approach:**
- Use `useState` for local component state
- Use `useContext` for shared state across multiple components
- Avoid global state management libraries for this simple application
- Context providers placed at page level

### Context Structure

```typescript
// contexts/AuthContext.tsx
interface AuthContextValue {
  accountId: string | null;
  accountName: string | null;
  token: string | null;
  isAuthenticated: boolean;
  login: (accountId: string, accountName: string, token: string) => void;
  logout: () => void;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [accountId, setAccountId] = useState<string | null>(null);
  const [accountName, setAccountName] = useState<string | null>(null);
  const [token, setToken] = useState<string | null>(null);

  const login = (id: string, name: string, authToken: string) => {
    setAccountId(id);
    setAccountName(name);
    setToken(authToken);
    localStorage.setItem('authToken', authToken);
  };

  const logout = () => {
    setAccountId(null);
    setAccountName(null);
    setToken(null);
    localStorage.removeItem('authToken');
  };

  return (
    <AuthContext.Provider value={{ accountId, accountName, token, isAuthenticated: !!token, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) throw new Error('useAuth must be used within AuthProvider');
  return context;
}
```

```typescript
// contexts/TransactionContext.tsx
interface TransactionContextValue {
  transactions: Transaction[];
  isLoading: boolean;
  error: Error | null;
  refreshTransactions: () => Promise<void>;
  addTransaction: (transaction: CreateTransactionRequest) => Promise<void>;
}

const TransactionContext = createContext<TransactionContextValue | undefined>(undefined);

export function TransactionProvider({ children }: { children: React.ReactNode }) {
  const [transactions, setTransactions] = useState<Transaction[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<Error | null>(null);
  const { accountId, token } = useAuth();

  const refreshTransactions = async () => {
    if (!accountId || !token) return;
    
    setIsLoading(true);
    setError(null);
    try {
      const response = await fetch('/api/transactions', {
        headers: { Authorization: `Bearer ${token}` },
      });
      if (!response.ok) throw new Error('Failed to fetch transactions');
      const data = await response.json();
      setTransactions(data);
    } catch (err) {
      setError(err instanceof Error ? err : new Error('Unknown error'));
    } finally {
      setIsLoading(false);
    }
  };

  const addTransaction = async (transaction: CreateTransactionRequest) => {
    if (!token) return;
    
    setIsLoading(true);
    setError(null);
    try {
      const response = await fetch('/api/transactions', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify(transaction),
      });
      if (!response.ok) throw new Error('Failed to create transaction');
      await refreshTransactions();
    } catch (err) {
      setError(err instanceof Error ? err : new Error('Unknown error'));
      throw err;
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    refreshTransactions();
  }, [accountId, token]);

  return (
    <TransactionContext.Provider value={{ transactions, isLoading, error, refreshTransactions, addTransaction }}>
      {children}
    </TransactionContext.Provider>
  );
}

export function useTransactions() {
  const context = useContext(TransactionContext);
  if (!context) throw new Error('useTransactions must be used within TransactionProvider');
  return context;
}
```

---

## Component Organization

### Directory Structure

```
src/
├── pages/                          # Top-level route components
│   ├── LoginPage.tsx              # Authentication entry point
│   ├── DashboardPage.tsx          # Main transaction view
│   └── NotFoundPage.tsx           # 404 page
│
├── components/
│   ├── shared/                    # Reusable UI components
│   │   ├── Button.tsx             # shadcn/ui wrapper
│   │   ├── Input.tsx              # shadcn/ui wrapper
│   │   ├── Card.tsx               # shadcn/ui wrapper
│   │   ├── Dialog.tsx             # shadcn/ui wrapper
│   │   ├── Form.tsx               # shadcn/ui wrapper
│   │   ├── AlertBanner.tsx        # Custom alert component
│   │   └── LoadingSpinner.tsx     # Custom loading component
│   │
│   ├── transactions/              # Transaction-specific components
│   │   ├── TransactionTable.tsx   # TanStack Table display
│   │   ├── TransactionRow.tsx     # Single transaction row
│   │   ├── AddTransactionForm.tsx # Create transaction form
│   │   └── TransactionFilters.tsx # Filter/sort controls
│   │
│   └── auth/                      # Authentication components
│       ├── LoginForm.tsx          # Login form
│       └── LogoutButton.tsx       # Logout button
│
├── contexts/                      # React Context providers
│   ├── AuthContext.tsx            # Authentication state
│   └── TransactionContext.tsx     # Transaction state
│
├── types/                         # TypeScript type definitions
│   ├── auth.types.ts              # Auth-related types
│   ├── transaction.types.ts       # Transaction types
│   └── api.types.ts               # API response types
│
├── hooks/                         # Custom React hooks
│   ├── useAuth.ts                 # Auth context hook
│   ├── useTransactions.ts         # Transaction context hook
│   └── useApi.ts                  # Generic API hook
│
├── utils/                         # Utility functions
│   ├── api.ts                     # API client setup
│   ├── formatting.ts              # Format dates, amounts
│   └── validation.ts              # Form validation
│
├── App.tsx                        # Root component
├── main.tsx                       # Entry point
└── index.css                      # Global styles
```

---

## Pages

### LoginPage.tsx

Entry point for authentication.

```typescript
export function LoginPage() {
  const navigate = useNavigate();
  const { login } = useAuth();
  const [error, setError] = useState<string | null>(null);

  const handleLogin = async (username: string, password: string) => {
    try {
      const response = await fetch('/api/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ username, password }),
      });

      if (!response.ok) {
        throw new Error('Invalid credentials');
      }

      const { accountId, accountName, token } = await response.json();
      login(accountId, accountName, token);
      navigate('/dashboard');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Login failed');
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50">
      <Card className="w-full max-w-md">
        <CardHeader>
          <CardTitle>Login to Checkbook</CardTitle>
        </CardHeader>
        <CardContent>
          {error && <AlertBanner severity="error" message={error} />}
          <LoginForm onSubmit={handleLogin} />
        </CardContent>
      </Card>
    </div>
  );
}
```

### DashboardPage.tsx

Main transaction display and management.

```typescript
export function DashboardPage() {
  const { accountName } = useAuth();
  const { transactions, isLoading, error } = useTransactions();
  const [showAddForm, setShowAddForm] = useState(false);

  if (error && !transactions.length) {
    return (
      <>
        <PageHeader title="Dashboard" />
        <AlertBanner severity="error" message={error.message} />
      </>
    );
  }

  return (
    <>
      <PageHeader title={`Welcome, ${accountName}`} />
      {error && transactions.length > 0 && (
        <AlertBanner severity="warning" message={error.message} />
      )}
      
      <div className="space-y-6">
        <div className="flex justify-between items-center">
          <h2 className="text-2xl font-bold">Transactions</h2>
          <Button onClick={() => setShowAddForm(!showAddForm)}>
            {showAddForm ? 'Cancel' : 'Add Transaction'}
          </Button>
        </div>

        {showAddForm && (
          <Card>
            <CardHeader>
              <CardTitle>New Transaction</CardTitle>
            </CardHeader>
            <CardContent>
              <AddTransactionForm onSuccess={() => setShowAddForm(false)} />
            </CardContent>
          </Card>
        )}

        {isLoading && <LoadingSpinner />}
        {!isLoading && <TransactionTable transactions={transactions} />}
      </div>
    </>
  );
}
```

---

## Reusable Components (shadcn/ui)

### Component Usage Pattern

```typescript
// components/shared/Button.tsx
import { Button as ShadcnButton } from '@/components/ui/button';

interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'default' | 'destructive' | 'outline' | 'secondary' | 'ghost' | 'link';
  size?: 'default' | 'sm' | 'lg';
  isLoading?: boolean;
}

export function Button({ 
  variant = 'default', 
  size = 'default', 
  isLoading = false, 
  children, 
  ...props 
}: ButtonProps) {
  return (
    <ShadcnButton variant={variant} size={size} disabled={isLoading} {...props}>
      {isLoading ? <LoadingSpinner /> : children}
    </ShadcnButton>
  );
}
```

### shadcn/ui Components to Use

- **Button** - All interactive buttons
- **Input** - Text inputs, email, password
- **Card** - Content containers
- **Dialog** - Modal dialogs
- **Form** - Form wrapper with validation
- **Select** - Dropdown selections
- **Alert** - Alert messages
- **Tabs** - Tab navigation (if needed)
- **Badge** - Status indicators

---

## Transaction Display with TanStack Table

### TransactionTable.tsx

```typescript
import { useReactTable, getCoreRowModel, getSortedRowModel, flexRender } from '@tanstack/react-table';

interface TransactionTableProps {
  transactions: Transaction[];
}

export function TransactionTable({ transactions }: TransactionTableProps) {
  const [sorting, setSorting] = useState<SortingState>([
    { id: 'date', desc: true }
  ]);

  const columns: ColumnDef<Transaction>[] = [
    {
      accessorKey: 'date',
      header: 'Date',
      cell: (info) => formatDate(info.getValue() as string),
    },
    {
      accessorKey: 'payee',
      header: 'Payee',
      cell: (info) => info.getValue(),
    },
    {
      accessorKey: 'amount',
      header: 'Amount',
      cell: (info) => formatCurrency(info.getValue() as number),
    },
    {
      accessorKey: 'balance',
      header: 'Balance',
      cell: (info) => formatCurrency(info.getValue() as number),
    },
  ];

  const table = useReactTable({
    data: transactions,
    columns,
    state: { sorting },
    onSortingChange: setSorting,
    getCoreRowModel: getCoreRowModel(),
    getSortedRowModel: getSortedRowModel(),
  });

  return (
    <div className="border rounded-lg overflow-hidden">
      <table className="w-full">
        <thead className="bg-gray-50">
          {table.getHeaderGroups().map((headerGroup) => (
            <tr key={headerGroup.id}>
              {headerGroup.headers.map((header) => (
                <th
                  key={header.id}
                  className="px-6 py-3 text-left text-sm font-semibold text-gray-900 cursor-pointer"
                  onClick={header.column.getToggleSortingHandler()}
                >
                  {flexRender(header.column.columnDef.header, header.getContext())}
                </th>
              ))}
            </tr>
          ))}
        </thead>
        <tbody>
          {table.getRowModel().rows.map((row) => (
            <tr key={row.id} className="border-t hover:bg-gray-50">
              {row.getVisibleCells().map((cell) => (
                <td key={cell.id} className="px-6 py-4 text-sm text-gray-700">
                  {flexRender(cell.column.columnDef.cell, cell.getContext())}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
```

**Features:**
- Sortable columns (click header to sort)
- Responsive layout
- Hover effects
- Pagination-ready (can add later)

---

## Forms with Validation

### AddTransactionForm.tsx

```typescript
interface AddTransactionFormProps {
  onSuccess: () => void;
}

export function AddTransactionForm({ onSuccess }: AddTransactionFormProps) {
  const { addTransaction } = useTransactions();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setIsSubmitting(true);
    setError(null);

    const formData = new FormData(event.currentTarget);
    const transaction: CreateTransactionRequest = {
      targetAccountId: formData.get('targetAccountId') as string,
      amount: parseFloat(formData.get('amount') as string),
      date: formData.get('date') as string,
    };

    try {
      await addTransaction(transaction);
      onSuccess();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create transaction');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      {error && <AlertBanner severity="error" message={error} />}
      
      <div>
        <label className="block text-sm font-medium text-gray-700">Recipient</label>
        <Input
          type="text"
          name="targetAccountId"
          placeholder="Select recipient account"
          required
        />
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700">Amount</label>
        <Input
          type="number"
          name="amount"
          placeholder="0.00"
          step="0.01"
          min="0"
          required
        />
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700">Date</label>
        <Input
          type="date"
          name="date"
          max={new Date().toISOString().split('T')[0]}
          required
        />
      </div>

      <Button type="submit" isLoading={isSubmitting}>
        Create Transaction
      </Button>
    </form>
  );
}
```

---

## Type Definitions

### types/transaction.types.ts

```typescript
export interface Transaction {
  id: string;
  sourceAccountId: string;
  targetAccountId: string;
  amount: number;
  date: string;
  createdAt: string;
}

export interface CreateTransactionRequest {
  targetAccountId: string;
  amount: number;
  date: string;
}

export interface Account {
  id: string;
  name: string;
}
```

### types/auth.types.ts

```typescript
export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  accountId: string;
  accountName: string;
  token: string;
}
```

---

## Utility Functions

### utils/formatting.ts

```typescript
export function formatCurrency(amount: number): string {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
  }).format(amount);
}

export function formatDate(date: string | Date): string {
  const d = typeof date === 'string' ? new Date(date) : date;
  return d.toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  });
}
```

### utils/api.ts

```typescript
export async function apiCall<T>(
  endpoint: string,
  options?: RequestInit
): Promise<T> {
  const token = localStorage.getItem('authToken');
  const headers: HeadersInit = {
    'Content-Type': 'application/json',
    ...options?.headers,
  };

  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }

  const response = await fetch(endpoint, {
    ...options,
    headers,
  });

  if (!response.ok) {
    throw new Error(`API error: ${response.statusText}`);
  }

  return response.json();
}
```

---

## Styling with TailwindCSS

### Global Styles (index.css)

```css
@tailwind base;
@tailwind components;
@tailwind utilities;

@layer components {
  .page-header {
    @apply mb-8 border-b pb-6;
  }

  .card-container {
    @apply bg-white rounded-lg shadow;
  }
}
```

---

## Key Patterns

### Error Handling

```typescript
// Critical: no data
if (error && !transactions.length) {
  return (
    <>
      <PageHeader title="Transactions" />
      <AlertBanner severity="error" message={error.message} />
    </>
  );
}

// Non-critical: operation failed
return (
  <>
    <PageHeader title="Transactions" />
    {error && transactions.length > 0 && (
      <AlertBanner severity="warning" message={error.message} />
    )}
    <TransactionTable transactions={transactions} />
  </>
);
```

### Loading States

```typescript
{isLoading && <LoadingSpinner />}
{!isLoading && transactions.length > 0 && <TransactionTable transactions={transactions} />}
{!isLoading && transactions.length === 0 && <EmptyState />}
```

---

## Dependencies

### Core
- `react` - UI library
- `react-dom` - DOM rendering
- `react-router-dom` - Routing

### UI & Styling
- `shadcn/ui` - Component library
- `@radix-ui/*` - Accessible components (shadcn dependency)
- `tailwindcss` - Utility CSS
- `lucide-react` - Icons

### Tables & Data
- `@tanstack/react-table` - Table management

### Utilities
- `typescript` - Type safety
- `vite` - Build tool

---

## Summary

The frontend architecture provides:
- **Simplicity:** Hooks and Context for straightforward state management
- **Reusability:** shadcn/ui components across pages
- **Type Safety:** Full TypeScript coverage
- **Performance:** TanStack Table for efficient rendering
- **Maintainability:** Clear separation of pages, components, and utilities
