"""
Database schema tests for the financial application.

Tests verify that all tables, constraints, and indexes are created correctly.
"""

import pytest
import psycopg2
from psycopg2 import sql
from typing import Any


@pytest.fixture
def db_connection():
    """Create a test database connection."""
    conn = psycopg2.connect(
        host="localhost",
        database="financial_app_test",
        user="postgres",
        password="postgres"
    )
    yield conn
    conn.close()


@pytest.fixture
def db_cursor(db_connection):
    """Create a database cursor."""
    cursor = db_connection.cursor()
    yield cursor
    cursor.close()


class TestAccountsTableSchema:
    """Test suite for Accounts table schema."""
    
    def test_accounts_table_exists(self, db_cursor):
        """Test A1: Accounts table exists with correct columns and constraints."""
        # Query to check if table exists
        db_cursor.execute("""
            SELECT EXISTS (
                SELECT FROM information_schema.tables 
                WHERE table_schema = 'public' 
                AND table_name = 'accounts'
            );
        """)
        
        table_exists = db_cursor.fetchone()[0]
        assert table_exists, "Accounts table should exist"
    
    def test_accounts_table_columns(self, db_cursor):
        """Test A1: Verify Accounts table has correct columns."""
        db_cursor.execute("""
            SELECT column_name, data_type, is_nullable, column_default
            FROM information_schema.columns
            WHERE table_name = 'accounts'
            ORDER BY ordinal_position;
        """)
        
        columns = db_cursor.fetchall()
        column_dict = {col[0]: col for col in columns}
        
        # Verify account_id column
        assert 'account_id' in column_dict, "account_id column should exist"
        assert column_dict['account_id'][1] == 'uuid', "account_id should be UUID type"
        assert column_dict['account_id'][2] == 'NO', "account_id should be NOT NULL"
        assert 'gen_random_uuid()' in column_dict['account_id'][3], "account_id should have default gen_random_uuid()"
        
        # Verify account_name column
        assert 'account_name' in column_dict, "account_name column should exist"
        assert column_dict['account_name'][1] == 'character varying', "account_name should be VARCHAR"
        assert column_dict['account_name'][2] == 'NO', "account_name should be NOT NULL"
        
        # Verify created_at column
        assert 'created_at' in column_dict, "created_at column should exist"
        assert 'timestamp' in column_dict['created_at'][1], "created_at should be TIMESTAMP"
        assert column_dict['created_at'][2] == 'NO', "created_at should be NOT NULL"
        
        # Verify updated_at column
        assert 'updated_at' in column_dict, "updated_at column should exist"
        assert 'timestamp' in column_dict['updated_at'][1], "updated_at should be TIMESTAMP"
        assert column_dict['updated_at'][2] == 'NO', "updated_at should be NOT NULL"
    
    def test_accounts_primary_key(self, db_cursor):
        """Test A1: Verify Accounts table has primary key on account_id."""
        db_cursor.execute("""
            SELECT constraint_name, constraint_type
            FROM information_schema.table_constraints
            WHERE table_name = 'accounts' AND constraint_type = 'PRIMARY KEY';
        """)
        
        constraints = db_cursor.fetchall()
        assert len(constraints) == 1, "Accounts table should have exactly one primary key"
        
        # Verify the primary key is on account_id
        db_cursor.execute("""
            SELECT column_name
            FROM information_schema.key_column_usage
            WHERE table_name = 'accounts' 
            AND constraint_name = %s;
        """, (constraints[0][0],))
        
        pk_columns = db_cursor.fetchall()
        assert len(pk_columns) == 1, "Primary key should be on single column"
        assert pk_columns[0][0] == 'account_id', "Primary key should be on account_id"


class TestTransactionsTableSchema:
    """Test suite for Transactions table schema."""
    
    def test_transactions_table_exists(self, db_cursor):
        """Test A2: Transactions table exists."""
        db_cursor.execute("""
            SELECT EXISTS (
                SELECT FROM information_schema.tables 
                WHERE table_schema = 'public' 
                AND table_name = 'transactions'
            );
        """)
        
        table_exists = db_cursor.fetchone()[0]
        assert table_exists, "Transactions table should exist"
    
    def test_transactions_table_columns(self, db_cursor):
        """Test A2: Verify Transactions table has correct columns."""
        db_cursor.execute("""
            SELECT column_name, data_type, is_nullable
            FROM information_schema.columns
            WHERE table_name = 'transactions'
            ORDER BY ordinal_position;
        """)
        
        columns = db_cursor.fetchall()
        column_dict = {col[0]: col for col in columns}
        
        # Verify all required columns exist
        required_columns = [
            'transaction_id', 'source_account_id', 'target_account_id',
            'amount', 'transaction_date', 'created_at'
        ]
        
        for col_name in required_columns:
            assert col_name in column_dict, f"{col_name} column should exist"
        
        # Verify data types
        assert column_dict['transaction_id'][1] == 'uuid', "transaction_id should be UUID"
        assert column_dict['source_account_id'][1] == 'uuid', "source_account_id should be UUID"
        assert column_dict['target_account_id'][1] == 'uuid', "target_account_id should be UUID"
        assert column_dict['amount'][1] == 'numeric', "amount should be NUMERIC/DECIMAL"
        assert column_dict['transaction_date'][1] == 'date', "transaction_date should be DATE"
        assert 'timestamp' in column_dict['created_at'][1], "created_at should be TIMESTAMP"
        
        # Verify NOT NULL constraints
        for col_name in required_columns:
            assert column_dict[col_name][2] == 'NO', f"{col_name} should be NOT NULL"
    
    def test_transactions_foreign_keys(self, db_cursor):
        """Test A2: Verify Transactions table has foreign key constraints."""
        db_cursor.execute("""
            SELECT
                tc.constraint_name,
                kcu.column_name,
                ccu.table_name AS foreign_table_name,
                ccu.column_name AS foreign_column_name
            FROM information_schema.table_constraints AS tc
            JOIN information_schema.key_column_usage AS kcu
                ON tc.constraint_name = kcu.constraint_name
            JOIN information_schema.constraint_column_usage AS ccu
                ON ccu.constraint_name = tc.constraint_name
            WHERE tc.table_name = 'transactions' 
            AND tc.constraint_type = 'FOREIGN KEY';
        """)
        
        foreign_keys = db_cursor.fetchall()
        assert len(foreign_keys) == 2, "Transactions should have 2 foreign keys"
        
        fk_columns = {fk[1] for fk in foreign_keys}
        assert 'source_account_id' in fk_columns, "Should have FK on source_account_id"
        assert 'target_account_id' in fk_columns, "Should have FK on target_account_id"
    
    def test_transactions_check_constraints(self, db_cursor):
        """Test A2: Verify Transactions table has check constraints."""
        db_cursor.execute("""
            SELECT constraint_name, check_clause
            FROM information_schema.check_constraints
            WHERE constraint_name IN (
                SELECT constraint_name
                FROM information_schema.table_constraints
                WHERE table_name = 'transactions'
            );
        """)
        
        check_constraints = db_cursor.fetchall()
        constraint_names = [cc[0] for cc in check_constraints]
        
        # Should have at least 2 check constraints (amount > 0 and source != target)
        assert len(check_constraints) >= 2, "Should have at least 2 check constraints"


class TestAccountSecurityTableSchema:
    """Test suite for AccountSecurity table schema."""
    
    def test_account_security_table_exists(self, db_cursor):
        """Test A3: AccountSecurity table exists."""
        db_cursor.execute("""
            SELECT EXISTS (
                SELECT FROM information_schema.tables 
                WHERE table_schema = 'public' 
                AND table_name = 'accountsecurity'
            );
        """)
        
        table_exists = db_cursor.fetchone()[0]
        assert table_exists, "AccountSecurity table should exist"
    
    def test_account_security_columns(self, db_cursor):
        """Test A3: Verify AccountSecurity table has correct columns."""
        db_cursor.execute("""
            SELECT column_name, data_type, is_nullable
            FROM information_schema.columns
            WHERE table_name = 'accountsecurity'
            ORDER BY ordinal_position;
        """)
        
        columns = db_cursor.fetchall()
        column_dict = {col[0]: col for col in columns}
        
        # Verify all required columns exist
        required_columns = [
            'security_id', 'account_id', 'username',
            'password_hash', 'created_at', 'last_login_at'
        ]
        
        for col_name in required_columns:
            assert col_name in column_dict, f"{col_name} column should exist"
        
        # Verify last_login_at is nullable
        assert column_dict['last_login_at'][2] == 'YES', "last_login_at should be nullable"
    
    def test_account_security_unique_constraints(self, db_cursor):
        """Test A3: Verify AccountSecurity has unique constraints."""
        db_cursor.execute("""
            SELECT constraint_name, column_name
            FROM information_schema.constraint_column_usage
            WHERE table_name = 'accountsecurity'
            AND constraint_name IN (
                SELECT constraint_name
                FROM information_schema.table_constraints
                WHERE table_name = 'accountsecurity'
                AND constraint_type = 'UNIQUE'
            );
        """)
        
        unique_constraints = db_cursor.fetchall()
        unique_columns = {uc[1] for uc in unique_constraints}
        
        assert 'account_id' in unique_columns, "account_id should be UNIQUE"
        assert 'username' in unique_columns, "username should be UNIQUE"


class TestDatabaseIndexes:
    """Test suite for database indexes."""
    
    def test_accounts_indexes(self, db_cursor):
        """Test A4: Verify Accounts table has required indexes."""
        db_cursor.execute("""
            SELECT indexname
            FROM pg_indexes
            WHERE tablename = 'accounts';
        """)
        
        indexes = [idx[0] for idx in db_cursor.fetchall()]
        
        # Should have index on account_name
        assert any('account_name' in idx.lower() for idx in indexes), \
            "Should have index on account_name"
    
    def test_transactions_indexes(self, db_cursor):
        """Test A4: Verify Transactions table has required indexes."""
        db_cursor.execute("""
            SELECT indexname
            FROM pg_indexes
            WHERE tablename = 'transactions';
        """)
        
        indexes = [idx[0] for idx in db_cursor.fetchall()]
        
        # Should have indexes on foreign keys and date
        assert any('source' in idx.lower() for idx in indexes), \
            "Should have index on source_account_id"
        assert any('target' in idx.lower() for idx in indexes), \
            "Should have index on target_account_id"
        assert any('date' in idx.lower() for idx in indexes), \
            "Should have index on transaction_date"
    
    def test_account_security_indexes(self, db_cursor):
        """Test A4: Verify AccountSecurity table has required indexes."""
        db_cursor.execute("""
            SELECT indexname
            FROM pg_indexes
            WHERE tablename = 'accountsecurity';
        """)
        
        indexes = [idx[0] for idx in db_cursor.fetchall()]
        
        # Should have index on username
        assert any('username' in idx.lower() for idx in indexes), \
            "Should have index on username"


class TestSchemaConstraintEnforcement:
    """Test suite for constraint enforcement."""
    
    def test_cannot_insert_negative_amount(self, db_connection, db_cursor):
        """Test A5: Cannot insert transaction with negative amount."""
        # This test will fail until we implement the schema
        # For now, we expect it to raise an exception
        with pytest.raises(psycopg2.IntegrityError):
            db_cursor.execute("""
                INSERT INTO transactions 
                (source_account_id, target_account_id, amount, transaction_date)
                VALUES 
                (gen_random_uuid(), gen_random_uuid(), -100.00, CURRENT_DATE);
            """)
            db_connection.commit()
        
        db_connection.rollback()
    
    def test_cannot_insert_self_transfer(self, db_connection, db_cursor):
        """Test A5: Cannot insert transaction with same source and target."""
        account_id = 'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11'
        
        with pytest.raises(psycopg2.IntegrityError):
            db_cursor.execute("""
                INSERT INTO transactions 
                (source_account_id, target_account_id, amount, transaction_date)
                VALUES 
                (%s, %s, 100.00, CURRENT_DATE);
            """, (account_id, account_id))
            db_connection.commit()
        
        db_connection.rollback()
