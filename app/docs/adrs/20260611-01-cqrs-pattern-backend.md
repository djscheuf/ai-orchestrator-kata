# 20260611-01. CQRS Pattern for Backend Architecture

Date: 2026-06-11

## Status

Accepted

## Context

The backend needs to handle both read and write operations for a financial transaction system. Traditional layered architecture can lead to tight coupling between read and write paths, making it difficult to scale them independently and test business logic in isolation.

## Decision

Implement CQRS (Command Query Responsibility Segregation) pattern with thin controllers, separating write operations (Commands) from read operations (Queries). The architecture consists of:

- **Controllers:** Handle HTTP concerns only (routing, status codes, serialization)
- **Commands & Queries:** Orchestrate operations and coordinate between layers
- **Services:** Implement business logic and enforce business rules
- **Repositories:** Abstract data access and database operations
- **MediatR:** Dispatch commands and queries to appropriate handlers

## Consequences

**Positive:**
- Clear separation between reads and writes enables independent scaling
- Business logic isolated from infrastructure concerns improves testability
- Single responsibility per layer reduces coupling
- Easier to add cross-cutting concerns (logging, caching) at handler level
- Commands and Queries can be tested independently with mocked repositories

**Negative:**
- Additional abstraction layers increase code complexity
- More files and classes to maintain compared to simpler patterns
- Requires discipline to keep controllers thin and avoid business logic leakage
- Learning curve for developers unfamiliar with CQRS pattern
