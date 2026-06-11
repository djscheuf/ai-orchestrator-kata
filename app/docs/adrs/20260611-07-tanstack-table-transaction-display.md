# 20260611-07. TanStack Table for Transaction Display

Date: 2026-06-11

## Status

Accepted

## Context

The application needs to display transactions in a table format with sorting, filtering, and pagination capabilities. The core requirement is to display 50-100 transactions at once without pagination, but the system should be extensible for future pagination needs. The team could build a custom table component or use a headless table library.

## Decision

Use TanStack Table (formerly React Table) for transaction display:

- **TanStack Table:** Headless, unstyled table library providing sorting, filtering, and pagination logic
- **Custom styling:** Apply shadcn/ui styling and TailwindCSS for visual presentation
- **Column definitions:** Define columns with accessors, headers, and custom cell renderers
- **Sorting state:** Manage sorting state with useState and pass to table instance

**Features:**
- Sortable columns (click header to sort)
- Responsive layout with TailwindCSS
- Hover effects for better UX
- Extensible for pagination, filtering, and selection

## Consequences

**Positive:**
- Headless approach provides complete control over styling and behavior
- Excellent performance with large datasets (virtual scrolling ready)
- Flexible column definitions enable custom cell rendering
- No styling dependencies (works with any CSS framework)
- Active development and community support
- Extensible for future features (pagination, filtering, selection)
- Lightweight compared to full-featured table libraries

**Negative:**
- Headless approach requires manual styling (no pre-built styles)
- Steeper learning curve compared to pre-styled table components
- Requires integration with other libraries for complete UI
- Sorting/filtering logic must be implemented in component
- No built-in pagination UI (requires custom implementation)
- Potential performance issues with very large datasets without virtual scrolling
