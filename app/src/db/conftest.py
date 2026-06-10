"""
Pytest configuration for database tests.

Sets up test database and handles cleanup.
"""

import pytest
import psycopg2
from psycopg2.extensions import ISOLATION_LEVEL_AUTOCOMMIT


@pytest.fixture(scope="session", autouse=True)
def setup_test_database():
    """Create test database before running tests."""
    # Connect to default postgres database to create test database
    conn = psycopg2.connect(
        host="localhost",
        database="postgres",
        user="postgres",
        password="postgres"
    )
    conn.set_isolation_level(ISOLATION_LEVEL_AUTOCOMMIT)
    cursor = conn.cursor()
    
    # Drop and recreate test database
    cursor.execute("DROP DATABASE IF EXISTS financial_app_test;")
    cursor.execute("CREATE DATABASE financial_app_test;")
    
    cursor.close()
    conn.close()
    
    yield
    
    # Cleanup after all tests
    conn = psycopg2.connect(
        host="localhost",
        database="postgres",
        user="postgres",
        password="postgres"
    )
    conn.set_isolation_level(ISOLATION_LEVEL_AUTOCOMMIT)
    cursor = conn.cursor()
    cursor.execute("DROP DATABASE IF EXISTS financial_app_test;")
    cursor.close()
    conn.close()


@pytest.fixture(scope="function", autouse=True)
def clean_database(db_connection):
    """Clean all tables before each test."""
    cursor = db_connection.cursor()
    
    # Drop all tables if they exist
    cursor.execute("""
        DROP TABLE IF EXISTS transactions CASCADE;
        DROP TABLE IF EXISTS accountsecurity CASCADE;
        DROP TABLE IF EXISTS accounts CASCADE;
    """)
    
    db_connection.commit()
    cursor.close()
    
    yield
