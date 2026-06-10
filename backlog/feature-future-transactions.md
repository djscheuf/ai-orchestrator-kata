# Feature 2: Future/Scheduled Transactions

## Feature Overview

Support scheduled payments with future dates. This feature allows users to plan and record transactions that will occur at a future date, enabling better financial planning and cash flow management.

## Functional Requirements

### Future Transaction Creation

#### Required Fields
- **Future date:** Date when the transaction will occur (must be in the future)
- **Amount:** Transaction amount (can be different from current account balance)
- **Target account:** Must be a known account within the bank system

#### Transaction Specification
- Users can specify any future date
- Amount does not have to match current balance or any existing transaction
- Target must be one of the existing accounts in the system (internal transfers only)

### Visibility and Privacy Rules

#### Source Account Visibility
- The account creating the future transaction (source) **CAN** see their scheduled transactions
- Source account should be able to view all their pending future transactions

#### Target Account Privacy
- The target account **CANNOT** see incoming scheduled transactions
- Future transactions remain invisible to the recipient until executed
- Privacy is maintained until the transaction date arrives

**Example:** If Account A schedules $200 to Account D for next month, only Account A can see this scheduled transaction. Account D will not see the pending $200 until the transaction is executed.

### Transaction Execution

While not explicitly detailed in the specification, implementation must consider:
- When/how future transactions become "real" transactions
- Automated execution on the scheduled date vs. manual triggering
- What happens if insufficient funds exist on execution date

## Non-Functional Requirements

No specific non-functional requirements were mentioned in the specification.

## Business Rules

### Privacy and Security
- **Strict privacy:** Target accounts must not see pending future transactions
- Only the source account has visibility into their scheduled payments

### Account Constraints
- Target account must exist in the system
- Target must be a known account (internal transfers only, no external accounts)

### Balance Considerations
- Future transactions can be scheduled for amounts exceeding current balance
- Validation of available funds may occur at execution time (not at scheduling time)

## Design Considerations

### Open Questions for Implementation

1. **Transaction Execution**
   - Automatic execution on scheduled date?
   - Manual trigger required?
   - Background job/scheduler needed?

2. **Insufficient Funds Handling**
   - What happens if balance is insufficient on execution date?
   - Transaction fails? Partial execution? Overdraft?

3. **Modification and Cancellation**
   - Can future transactions be edited?
   - Can they be cancelled before execution?
   - What audit trail is needed?

4. **Display and UI**
   - How are future transactions displayed differently from completed transactions?
   - Separate view or integrated with main transaction list?
   - Visual indicators for "pending" status?

5. **Recurring Transactions**
   - Are recurring scheduled payments in scope?
   - Or only one-time future transactions?

## Dependencies

### Required Features
- Core transaction system (to execute future transactions)
- Account management and validation

### Feature Interactions
- **Transaction Categorization (Feature 1):** Future transactions may need categories
- **Transaction Search (Feature 3):** Search may need to filter/include future transactions
- **Core Balance Calculation:** Must account for whether future transactions affect "available balance"

## Implementation Considerations

### Data Model Changes
- Add future transaction entity or status field
- Fields: scheduled_date, source_account, target_account, amount, status (pending/executed/cancelled)
- Relationship to regular transactions (separate table or status flag?)

### System Architecture
- May require scheduled job/cron for automatic execution
- Background processing for transaction execution
- Notification system for executed transactions?

## Success Criteria

1. Users can create future-dated transactions with valid target accounts
2. Source account can view all their scheduled future transactions
3. Target account cannot see pending incoming future transactions
4. Future transactions are clearly distinguished from completed transactions
5. System enforces privacy rules (target cannot see pending transactions)
6. Future transactions can be executed/converted to regular transactions on the scheduled date
