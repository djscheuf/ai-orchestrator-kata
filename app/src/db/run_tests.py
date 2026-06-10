"""
Simple test runner for schema validation tests.

Runs tests without requiring pytest installation.
"""

import re
from pathlib import Path
import sys


class TestResult:
    """Represents a test result."""
    def __init__(self, name, passed, error=None):
        self.name = name
        self.passed = passed
        self.error = error


class SimpleTestRunner:
    """Simple test runner that mimics pytest behavior."""
    
    def __init__(self):
        self.results = []
        self.schema_sql = self._load_schema()
    
    def _load_schema(self):
        """Load the schema.sql file."""
        schema_path = Path(__file__).parent / "schema.sql"
        with open(schema_path, 'r') as f:
            return f.read()
    
    def run_test(self, test_func, test_name):
        """Run a single test function."""
        try:
            test_func(self.schema_sql)
            self.results.append(TestResult(test_name, True))
            print(f"✓ {test_name}")
            return True
        except AssertionError as e:
            self.results.append(TestResult(test_name, False, str(e)))
            print(f"✗ {test_name}")
            print(f"  Error: {e}")
            return False
        except Exception as e:
            self.results.append(TestResult(test_name, False, f"Unexpected error: {e}"))
            print(f"✗ {test_name}")
            print(f"  Unexpected error: {e}")
            return False
    
    def print_summary(self):
        """Print test summary."""
        passed = sum(1 for r in self.results if r.passed)
        failed = sum(1 for r in self.results if not r.passed)
        total = len(self.results)
        
        print("\n" + "="*60)
        print(f"Test Summary: {passed} passed, {failed} failed, {total} total")
        print("="*60)
        
        if failed > 0:
            print("\nFailed tests:")
            for result in self.results:
                if not result.passed:
                    print(f"  - {result.name}")
        
        return failed == 0


# Test functions for Accounts table
def test_accounts_table_defined(schema_sql):
    """Test A1: Accounts table is defined in schema."""
    assert 'CREATE TABLE Accounts' in schema_sql or 'CREATE TABLE accounts' in schema_sql, \
        "Schema should define Accounts table"


def test_accounts_has_account_id_column(schema_sql):
    """Test A1: Accounts table has account_id column."""
    pattern = r'account_id\s+UUID'
    assert re.search(pattern, schema_sql, re.IGNORECASE), \
        "Accounts table should have account_id UUID column"


def test_accounts_has_account_name_column(schema_sql):
    """Test A1: Accounts table has account_name column."""
    pattern = r'account_name\s+VARCHAR'
    assert re.search(pattern, schema_sql, re.IGNORECASE), \
        "Accounts table should have account_name VARCHAR column"


def test_accounts_has_timestamps(schema_sql):
    """Test A1: Accounts table has created_at and updated_at columns."""
    assert re.search(r'created_at\s+TIMESTAMP', schema_sql, re.IGNORECASE), \
        "Accounts table should have created_at TIMESTAMP column"
    assert re.search(r'updated_at\s+TIMESTAMP', schema_sql, re.IGNORECASE), \
        "Accounts table should have updated_at TIMESTAMP column"


def test_accounts_has_primary_key(schema_sql):
    """Test A1: Accounts table has PRIMARY KEY constraint."""
    pattern = r'account_id.*PRIMARY KEY|PRIMARY KEY.*\(account_id\)'
    assert re.search(pattern, schema_sql, re.IGNORECASE), \
        "Accounts table should have PRIMARY KEY on account_id"


# Test functions for Transactions table
def test_transactions_table_defined(schema_sql):
    """Test A2: Transactions table is defined in schema."""
    assert 'CREATE TABLE Transactions' in schema_sql or 'CREATE TABLE transactions' in schema_sql, \
        "Schema should define Transactions table"


def test_transactions_has_required_columns(schema_sql):
    """Test A2: Transactions table has all required columns."""
    required_columns = [
        'transaction_id',
        'source_account_id',
        'target_account_id',
        'amount',
        'transaction_date',
        'created_at'
    ]
    
    for column in required_columns:
        assert re.search(rf'{column}\s+\w+', schema_sql, re.IGNORECASE), \
            f"Transactions table should have {column} column"


def test_transactions_has_foreign_keys(schema_sql):
    """Test A2: Transactions table has foreign key constraints."""
    assert re.search(r'FOREIGN KEY.*source_account_id|source_account_id.*REFERENCES', 
                    schema_sql, re.IGNORECASE), \
        "Transactions should have FK on source_account_id"
    assert re.search(r'FOREIGN KEY.*target_account_id|target_account_id.*REFERENCES', 
                    schema_sql, re.IGNORECASE), \
        "Transactions should have FK on target_account_id"


def test_transactions_has_amount_check_constraint(schema_sql):
    """Test A2: Transactions table has CHECK constraint for positive amount."""
    pattern = r'CHECK.*amount\s*>\s*0|CONSTRAINT.*amount.*>\s*0'
    assert re.search(pattern, schema_sql, re.IGNORECASE), \
        "Transactions should have CHECK constraint: amount > 0"


def test_transactions_has_different_accounts_check(schema_sql):
    """Test A2: Transactions table prevents self-transfers."""
    pattern = r'CHECK.*source_account_id\s*!=\s*target_account_id|source_account_id\s*<>\s*target_account_id'
    assert re.search(pattern, schema_sql, re.IGNORECASE), \
        "Transactions should have CHECK constraint: source_account_id != target_account_id"


# Test functions for AccountSecurity table
def test_account_security_table_defined(schema_sql):
    """Test A3: AccountSecurity table is defined in schema."""
    assert 'CREATE TABLE AccountSecurity' in schema_sql or 'CREATE TABLE accountsecurity' in schema_sql, \
        "Schema should define AccountSecurity table"


def test_account_security_has_required_columns(schema_sql):
    """Test A3: AccountSecurity table has all required columns."""
    required_columns = [
        'security_id',
        'account_id',
        'username',
        'password_hash',
        'created_at',
        'last_login_at'
    ]
    
    for column in required_columns:
        assert re.search(rf'{column}\s+\w+', schema_sql, re.IGNORECASE), \
            f"AccountSecurity table should have {column} column"


def test_account_security_has_unique_constraints(schema_sql):
    """Test A3: AccountSecurity has UNIQUE constraints."""
    assert re.search(r'account_id.*UNIQUE|UNIQUE.*account_id', schema_sql, re.IGNORECASE), \
        "AccountSecurity should have UNIQUE constraint on account_id"
    assert re.search(r'username.*UNIQUE|UNIQUE.*username', schema_sql, re.IGNORECASE), \
        "AccountSecurity should have UNIQUE constraint on username"


def test_account_security_has_foreign_key(schema_sql):
    """Test A3: AccountSecurity has foreign key to Accounts."""
    pattern = r'FOREIGN KEY.*account_id|account_id.*REFERENCES.*Accounts'
    assert re.search(pattern, schema_sql, re.IGNORECASE), \
        "AccountSecurity should have FK to Accounts table"


# Test functions for indexes
def test_accounts_indexes_defined(schema_sql):
    """Test A4: Indexes on Accounts table are defined."""
    pattern = r'CREATE INDEX.*accounts.*account_name|CREATE INDEX.*account_name.*accounts'
    assert re.search(pattern, schema_sql, re.IGNORECASE), \
        "Should have index on Accounts.account_name"


def test_transactions_indexes_defined(schema_sql):
    """Test A4: Indexes on Transactions table are defined."""
    assert re.search(r'CREATE INDEX.*transactions.*source', schema_sql, re.IGNORECASE), \
        "Should have index on Transactions.source_account_id"
    assert re.search(r'CREATE INDEX.*transactions.*target', schema_sql, re.IGNORECASE), \
        "Should have index on Transactions.target_account_id"
    assert re.search(r'CREATE INDEX.*transactions.*date', schema_sql, re.IGNORECASE), \
        "Should have index on Transactions.transaction_date"


def test_account_security_indexes_defined(schema_sql):
    """Test A4: Indexes on AccountSecurity table are defined."""
    pattern = r'CREATE INDEX.*accountsecurity.*username|CREATE INDEX.*username.*accountsecurity'
    assert re.search(pattern, schema_sql, re.IGNORECASE), \
        "Should have index on AccountSecurity.username"


def test_all_tables_defined(schema_sql):
    """Test: All three core tables are defined."""
    tables = ['Accounts', 'Transactions', 'AccountSecurity']
    for table in tables:
        assert re.search(rf'CREATE TABLE {table}', schema_sql, re.IGNORECASE), \
            f"{table} table should be defined"


def main():
    """Run all tests."""
    runner = SimpleTestRunner()
    
    print("Running Database Schema Tests")
    print("="*60)
    print("\nPhase: RED - Verifying tests fail before implementation\n")
    
    # Define all tests
    tests = [
        (test_accounts_table_defined, "Accounts table is defined"),
        (test_accounts_has_account_id_column, "Accounts has account_id UUID column"),
        (test_accounts_has_account_name_column, "Accounts has account_name VARCHAR column"),
        (test_accounts_has_timestamps, "Accounts has timestamp columns"),
        (test_accounts_has_primary_key, "Accounts has PRIMARY KEY"),
        (test_transactions_table_defined, "Transactions table is defined"),
        (test_transactions_has_required_columns, "Transactions has all required columns"),
        (test_transactions_has_foreign_keys, "Transactions has foreign keys"),
        (test_transactions_has_amount_check_constraint, "Transactions has amount CHECK constraint"),
        (test_transactions_has_different_accounts_check, "Transactions prevents self-transfers"),
        (test_account_security_table_defined, "AccountSecurity table is defined"),
        (test_account_security_has_required_columns, "AccountSecurity has all required columns"),
        (test_account_security_has_unique_constraints, "AccountSecurity has UNIQUE constraints"),
        (test_account_security_has_foreign_key, "AccountSecurity has foreign key"),
        (test_accounts_indexes_defined, "Accounts indexes are defined"),
        (test_transactions_indexes_defined, "Transactions indexes are defined"),
        (test_account_security_indexes_defined, "AccountSecurity indexes are defined"),
        (test_all_tables_defined, "All three core tables are defined"),
    ]
    
    # Run all tests
    for test_func, test_name in tests:
        runner.run_test(test_func, test_name)
    
    # Print summary
    all_passed = runner.print_summary()
    
    if not all_passed:
        print("\n✓ RED phase confirmed: Tests are failing as expected.")
        print("Next step: Implement schema.sql to make tests pass (GREEN phase)")
        sys.exit(1)
    else:
        print("\n✓ All tests passed!")
        sys.exit(0)


if __name__ == "__main__":
    main()
