"""
Script to apply database schema to test database.
"""

import psycopg2


def apply_schema():
    """Apply schema.sql to the test database."""
    conn = psycopg2.connect(
        host="localhost",
        database="financial_app_test",
        user="postgres",
        password="postgres"
    )
    
    cursor = conn.cursor()
    
    # Read and execute schema.sql
    with open('schema.sql', 'r') as f:
        schema_sql = f.read()
        cursor.execute(schema_sql)
    
    conn.commit()
    cursor.close()
    conn.close()
    
    print("Schema applied successfully!")


if __name__ == "__main__":
    apply_schema()
