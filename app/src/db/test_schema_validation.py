"""
Schema validation tests - validates SQL schema file structure.

These tests parse and validate the schema.sql file without requiring a live database.
"""

import pytest
import re
from pathlib import Path


@pytest.fixture
def schema_sql():
    """Load the schema.sql file."""
    schema_path = Path(__file__).parent / "schema.sql"
    with open(schema_path, 'r') as f:
        return f.read()


class TestAccountsTableSchema:
    """Test suite for Accounts table schema definition."""
    
    def test_accounts_table_defined(self, schema_sql):
        """Test A1: Accounts table is defined in schema."""
        assert 'CREATE TABLE Accounts' in schema_sql or 'CREATE TABLE accounts' in schema_sql, \
            "Schema should define Accounts table"
    
    def test_accounts_has_account_id_column(self, schema_sql):
        """Test A1: Accounts table has account_id column."""
        # Look for account_id definition
        pattern = r'account_id\s+UUID'
        assert re.search(pattern, schema_sql, re.IGNORECASE), \
            "Accounts table should have account_id UUID column"
    
    def test_accounts_has_account_name_column(self, schema_sql):
        """Test A1: Accounts table has account_name column."""
        pattern = r'account_name\s+VARCHAR'
        assert re.search(pattern, schema_sql, re.IGNORECASE), \
            "Accounts table should have account_name VARCHAR column"
    
    def test_accounts_has_timestamps(self, schema_sql):
        """Test A1: Accounts table has created_at and updated_at columns."""
        assert re.search(r'created_at\s+TIMESTAMP', schema_sql, re.IGNORECASE), \
            "Accounts table should have created_at TIMESTAMP column"
        assert re.search(r'updated_at\s+TIMESTAMP', schema_sql, re.IGNORECASE), \
            "Accounts table should have updated_at TIMESTAMP column"
    
    def test_accounts_has_primary_key(self, schema_sql):
        """Test A1: Accounts table has PRIMARY KEY constraint."""
        # Look for PRIMARY KEY definition
        pattern = r'account_id.*PRIMARY KEY|PRIMARY KEY.*\(account_id\)'
        assert re.search(pattern, schema_sql, re.IGNORECASE), \
            "Accounts table should have PRIMARY KEY on account_id"


class TestTransactionsTableSchema:
    """Test suite for Transactions table schema definition."""
    
    def test_transactions_table_defined(self, schema_sql):
        """Test A2: Transactions table is defined in schema."""
        assert 'CREATE TABLE Transactions' in schema_sql or 'CREATE TABLE transactions' in schema_sql, \
            "Schema should define Transactions table"
    
    def test_transactions_has_required_columns(self, schema_sql):
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
    
    def test_transactions_has_foreign_keys(self, schema_sql):
        """Test A2: Transactions table has foreign key constraints."""
        # Look for FOREIGN KEY or REFERENCES
        assert re.search(r'FOREIGN KEY.*source_account_id|source_account_id.*REFERENCES', 
                        schema_sql, re.IGNORECASE), \
            "Transactions should have FK on source_account_id"
        assert re.search(r'FOREIGN KEY.*target_account_id|target_account_id.*REFERENCES', 
                        schema_sql, re.IGNORECASE), \
            "Transactions should have FK on target_account_id"
    
    def test_transactions_has_amount_check_constraint(self, schema_sql):
        """Test A2: Transactions table has CHECK constraint for positive amount."""
        pattern = r'CHECK.*amount\s*>\s*0|CONSTRAINT.*amount.*>\s*0'
        assert re.search(pattern, schema_sql, re.IGNORECASE), \
            "Transactions should have CHECK constraint: amount > 0"
    
    def test_transactions_has_different_accounts_check(self, schema_sql):
        """Test A2: Transactions table prevents self-transfers."""
        pattern = r'CHECK.*source_account_id\s*!=\s*target_account_id|source_account_id\s*<>\s*target_account_id'
        assert re.search(pattern, schema_sql, re.IGNORECASE), \
            "Transactions should have CHECK constraint: source_account_id != target_account_id"


class TestAccountSecurityTableSchema:
    """Test suite for AccountSecurity table schema definition."""
    
    def test_account_security_table_defined(self, schema_sql):
        """Test A3: AccountSecurity table is defined in schema."""
        assert 'CREATE TABLE AccountSecurity' in schema_sql or 'CREATE TABLE accountsecurity' in schema_sql, \
            "Schema should define AccountSecurity table"
    
    def test_account_security_has_required_columns(self, schema_sql):
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
    
    def test_account_security_has_unique_constraints(self, schema_sql):
        """Test A3: AccountSecurity has UNIQUE constraints."""
        # Look for UNIQUE on account_id and username
        assert re.search(r'account_id.*UNIQUE|UNIQUE.*account_id', schema_sql, re.IGNORECASE), \
            "AccountSecurity should have UNIQUE constraint on account_id"
        assert re.search(r'username.*UNIQUE|UNIQUE.*username', schema_sql, re.IGNORECASE), \
            "AccountSecurity should have UNIQUE constraint on username"
    
    def test_account_security_has_foreign_key(self, schema_sql):
        """Test A3: AccountSecurity has foreign key to Accounts."""
        pattern = r'FOREIGN KEY.*account_id|account_id.*REFERENCES.*Accounts'
        assert re.search(pattern, schema_sql, re.IGNORECASE), \
            "AccountSecurity should have FK to Accounts table"


class TestDatabaseIndexes:
    """Test suite for database indexes."""
    
    def test_accounts_indexes_defined(self, schema_sql):
        """Test A4: Indexes on Accounts table are defined."""
        pattern = r'CREATE INDEX.*accounts.*account_name|CREATE INDEX.*account_name.*accounts'
        assert re.search(pattern, schema_sql, re.IGNORECASE), \
            "Should have index on Accounts.account_name"
    
    def test_transactions_indexes_defined(self, schema_sql):
        """Test A4: Indexes on Transactions table are defined."""
        # Check for indexes on source_account_id, target_account_id, and transaction_date
        assert re.search(r'CREATE INDEX.*transactions.*source', schema_sql, re.IGNORECASE), \
            "Should have index on Transactions.source_account_id"
        assert re.search(r'CREATE INDEX.*transactions.*target', schema_sql, re.IGNORECASE), \
            "Should have index on Transactions.target_account_id"
        assert re.search(r'CREATE INDEX.*transactions.*date', schema_sql, re.IGNORECASE), \
            "Should have index on Transactions.transaction_date"
    
    def test_account_security_indexes_defined(self, schema_sql):
        """Test A4: Indexes on AccountSecurity table are defined."""
        pattern = r'CREATE INDEX.*accountsecurity.*username|CREATE INDEX.*username.*accountsecurity'
        assert re.search(pattern, schema_sql, re.IGNORECASE), \
            "Should have index on AccountSecurity.username"


class TestSchemaCompleteness:
    """Test suite for overall schema completeness."""
    
    def test_all_tables_defined(self, schema_sql):
        """Test: All three core tables are defined."""
        tables = ['Accounts', 'Transactions', 'AccountSecurity']
        for table in tables:
            assert re.search(rf'CREATE TABLE {table}', schema_sql, re.IGNORECASE), \
                f"{table} table should be defined"
    
    def test_schema_has_comments(self, schema_sql):
        """Test: Schema includes documentation comments."""
        # Schema should have some comments explaining the structure
        assert '--' in schema_sql or '/*' in schema_sql, \
            "Schema should include documentation comments"
