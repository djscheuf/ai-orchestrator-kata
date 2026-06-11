# 20260611-03. Tech Stack Selection

Date: 2026-06-11

## Status

Accepted

## Context

The project is an AI code generation benchmark ("Orchestrator Kata") evaluating AI orchestrators by tracking PR size per feature. The application is a simple financial/banking checkbook with closed system simulation and basic accounting. The team needs to select technologies that balance simplicity, maintainability, and industry standards.

## Decision

Select the following technology stack:

| Layer | Technology | Purpose |
|-------|------------|---------|
| Frontend | React + TypeScript | User interface with type safety |
| Backend API | .NET | HTTP API and business logic |
| Database | SQL (PostgreSQL) | Data persistence with relational model |
| Analytics Sidecar | Python | Data analytics and reporting |

**Rationale:**
- React + TypeScript: Industry standard for modern web UIs with strong type safety
- .NET: Mature framework with excellent support for CQRS patterns and dependency injection
- SQL: Proven relational database suitable for financial transactions with ACID guarantees
- Python: Flexible for analytics and data processing tasks

## Consequences

**Positive:**
- Well-established technologies with large communities and extensive documentation
- .NET provides excellent tooling for CQRS and layered architecture patterns
- React ecosystem mature with extensive UI component libraries (shadcn/ui, TailwindCSS)
- SQL databases provide ACID guarantees critical for financial transactions
- Python enables rapid development of analytics features
- Clear separation of concerns across frontend, backend, and analytics layers

**Negative:**
- Requires team expertise across multiple languages (C#, TypeScript, Python, SQL)
- Increased operational complexity managing multiple runtime environments
- Potential latency between frontend and backend API calls
- Python analytics sidecar adds deployment complexity
- Higher infrastructure costs compared to single-language monolith
