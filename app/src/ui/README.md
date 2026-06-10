# Financial Dashboard UI

React frontend for the financial transaction management application.

## Features

- **Authentication**: Login with username and password
- **Transaction Management**: View and create transactions
- **Balance Display**: Real-time account balance calculation
- **Transaction List**: Sortable table with TanStack Table
- **Session Persistence**: Maintains login state across page refreshes

## Architecture

### State Management

- **AuthContext**: Manages authentication state and login/logout
- **TransactionContext**: Manages transaction list and balance state

### Components

- **Dashboard**: Main application layout with routing
- **LoginForm**: User authentication form
- **TransactionList**: Displays transactions in a sortable table
- **BalanceDisplay**: Shows current account balance
- **TransactionForm**: Form for creating new transactions

## Setup

### Prerequisites

- Node.js 18+
- npm or yarn

### Installation

```bash
npm install
```

### Environment Variables

Create a `.env` file based on `.env.example`:

```bash
REACT_APP_API_URL=http://localhost:5000/api
```

### Development

```bash
npm run dev
```

The application will be available at `http://localhost:3000`

### Testing

```bash
npm test
npm run test:watch
npm run test:coverage
```

### Build

```bash
npm run build
```

## Project Structure

```
app/src/ui/
├── components/          # React components
│   ├── Dashboard.tsx
│   ├── LoginForm.tsx
│   ├── TransactionList.tsx
│   ├── BalanceDisplay.tsx
│   └── TransactionForm.tsx
├── contexts/            # React Context providers
│   ├── AuthContext.tsx
│   └── TransactionContext.tsx
├── __tests__/           # Test files
├── api-client.ts        # API client for backend communication
├── types.ts             # TypeScript type definitions
├── App.tsx              # Root component
├── App.css              # Global styles
└── index.tsx            # Entry point
```

## API Integration

The UI communicates with the backend API at the configured `REACT_APP_API_URL`.

### API Endpoints Used

- `POST /api/auth/login` - User authentication
- `GET /api/transactions` - Fetch transactions
- `POST /api/transactions` - Create new transaction
- `GET /api/accounts/balance` - Get account balance

## Testing Strategy

Tests are written using Jest and React Testing Library following TDD principles:

- Unit tests for components and context hooks
- Integration tests for component interactions
- Mock API client for isolated testing

## Performance Considerations

- Transactions are fetched once on dashboard load
- Client-side sorting using TanStack Table
- Balance calculated from transaction list
- No pagination (handles up to 100 transactions)

## Browser Support

- Chrome (latest)
- Firefox (latest)
- Safari (latest)
- Edge (latest)
