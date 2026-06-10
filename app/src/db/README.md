# Database Layer

This directory contains the database schema, migrations, and tests for the financial application.

## Prerequisites

- PostgreSQL 14+ installed and running
- Python 3.10+
- PostgreSQL user `postgres` with password `postgres` (or update connection strings)

## Setup

1. Install Python dependencies:
```bash
pip install -r requirements.txt
```

2. Ensure PostgreSQL is running:
```bash
# Check PostgreSQL status
sudo systemctl status postgresql

# Start if not running
sudo systemctl start postgresql
```

## Running Tests

The tests will automatically create a test database, apply the schema, and run all tests:

```bash
pytest -v
```

## Manual Schema Application

To manually apply the schema to the test database:

```bash
python apply_schema.py
```

## Test Structure

- `test_schema.py` - Tests for database schema (tables, constraints, indexes)
- `test_seed_data.py` - Tests for seed data generation
- `conftest.py` - Pytest configuration and fixtures
- `schema.sql` - Complete database schema definition
- `seed_data.sql` - Seed data for test accounts and transactions

## TDD Workflow

1. **RED**: Write failing test
2. **GREEN**: Implement minimal schema to pass test
3. **REFACTOR**: Clean up and optimize

Current phase: **RED** - Tests are written but schema is not yet implemented.
