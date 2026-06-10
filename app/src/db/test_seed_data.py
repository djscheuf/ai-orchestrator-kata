"""
Seed data validation tests.

Tests verify that seed data SQL creates the expected accounts and transactions.
"""

import re
from pathlib import Path


class SeedDataValidator:
    """Validates seed data SQL file."""
    
    def __init__(self):
        self.seed_sql = self._load_seed_data()
    
    def _load_seed_data(self):
        """Load the seed_data.sql file."""
        seed_path = Path(__file__).parent / "seed_data.sql"
        with open(seed_path, 'r') as f:
            return f.read()
    
    def run_test(self, test_func, test_name):
        """Run a single test function."""
        try:
            test_func(self.seed_sql)
            print(f"✓ {test_name}")
            return True
        except AssertionError as e:
            print(f"✗ {test_name}")
            print(f"  Error: {e}")
            return False
        except Exception as e:
            print(f"✗ {test_name}")
            print(f"  Unexpected error: {e}")
            return False


# Test functions for seed data
def test_accounts_insert_exists(seed_sql):
    """Test B1: Seed data includes INSERT for Accounts table."""
    pattern = r'INSERT INTO Accounts|INSERT INTO accounts'
    assert re.search(pattern, seed_sql, re.IGNORECASE), \
        "Seed data should include INSERT INTO Accounts"


def test_five_accounts_created(seed_sql):
    """Test B1: Seed data creates 5 test accounts."""
    # Look for account names: Alice, Bob, Charlie, Diana, Eve
    accounts = ['Alice', 'Bob', 'Charlie', 'Diana', 'Eve']
    for account in accounts:
        assert account in seed_sql, \
            f"Seed data should include account for {account}"


def test_account_security_insert_exists(seed_sql):
    """Test B2: Seed data includes INSERT for AccountSecurity table."""
    pattern = r'INSERT INTO AccountSecurity|INSERT INTO accountsecurity'
    assert re.search(pattern, seed_sql, re.IGNORECASE), \
        "Seed data should include INSERT INTO AccountSecurity"


def test_five_security_records_created(seed_sql):
    """Test B2: Seed data creates 5 security records."""
    # Look for usernames: user_a, user_b, user_c, user_d, user_e
    usernames = ['user_a', 'user_b', 'user_c', 'user_d', 'user_e']
    for username in usernames:
        assert username in seed_sql, \
            f"Seed data should include username {username}"


def test_password_hashes_present(seed_sql):
    """Test B2: Seed data includes password hashes."""
    # Look for bcrypt hash pattern ($2b$ or $2a$)
    pattern = r'\$2[ab]\$'
    assert re.search(pattern, seed_sql), \
        "Seed data should include bcrypt password hashes"


def test_transactions_insert_exists(seed_sql):
    """Test B3: Seed data includes INSERT for Transactions table."""
    pattern = r'INSERT INTO Transactions|INSERT INTO transactions'
    assert re.search(pattern, seed_sql, re.IGNORECASE), \
        "Seed data should include INSERT INTO Transactions"


def test_multiple_transactions_created(seed_sql):
    """Test B3: Seed data creates multiple transactions (at least 20)."""
    # Count transaction value tuples - look for patterns like (id, id, amount, date)
    # Match tuples with UUID variables or function calls and amounts
    pattern = r'\([^)]*_id[^)]*,\s*[^)]*_id[^)]*,\s*[\d.]+[^)]*\)'
    matches = re.findall(pattern, seed_sql)
    
    assert len(matches) >= 20, \
        f"Seed data should create at least 20 transactions, found {len(matches)}"


def test_seed_data_has_comments(seed_sql):
    """Test: Seed data includes documentation comments."""
    assert '--' in seed_sql or '/*' in seed_sql, \
        "Seed data should include documentation comments"


def main():
    """Run all seed data tests."""
    validator = SeedDataValidator()
    
    print("\nRunning Seed Data Tests")
    print("="*60)
    print("\nPhase: RED - Verifying seed data tests fail before implementation\n")
    
    tests = [
        (test_accounts_insert_exists, "Accounts INSERT statement exists"),
        (test_five_accounts_created, "Five test accounts are created"),
        (test_account_security_insert_exists, "AccountSecurity INSERT statement exists"),
        (test_five_security_records_created, "Five security records are created"),
        (test_password_hashes_present, "Password hashes are present"),
        (test_transactions_insert_exists, "Transactions INSERT statement exists"),
        (test_multiple_transactions_created, "At least 20 transactions are created"),
        (test_seed_data_has_comments, "Seed data has documentation comments"),
    ]
    
    passed = 0
    failed = 0
    
    for test_func, test_name in tests:
        if validator.run_test(test_func, test_name):
            passed += 1
        else:
            failed += 1
    
    print("\n" + "="*60)
    print(f"Test Summary: {passed} passed, {failed} failed, {passed + failed} total")
    print("="*60)
    
    if failed > 0:
        print("\n✓ RED phase confirmed: Seed data tests are failing as expected.")
        print("Next step: Implement seed_data.sql to make tests pass (GREEN phase)")
        return 1
    else:
        print("\n✓ All seed data tests passed!")
        return 0


if __name__ == "__main__":
    import sys
    sys.exit(main())
