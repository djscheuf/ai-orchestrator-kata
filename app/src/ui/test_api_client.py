"""
API Client validation tests for TypeScript frontend.

Tests verify that TypeScript API client file exists and has correct structure.
"""

from pathlib import Path


class ApiClientValidator:
    """Validates that API client file exists and has correct structure."""
    
    def __init__(self):
        self.ui_dir = Path(__file__).parent
    
    def run_test(self, test_func, test_name):
        """Run a single test function."""
        try:
            test_func(self.ui_dir)
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


# Test functions for API client
def test_api_client_file_exists(ui_dir):
    """Test E1: api-client.ts file exists."""
    api_client = ui_dir / "api-client.ts"
    assert api_client.exists(), "api-client.ts should exist"


def test_api_client_has_login_method(ui_dir):
    """Test E1: API client has login method."""
    api_client = ui_dir / "api-client.ts"
    
    with open(api_client, 'r') as f:
        content = f.read()
    
    assert "login" in content.lower(), \
        "API client should have login method"


def test_api_client_has_get_transactions_method(ui_dir):
    """Test E2: API client has getTransactions method."""
    api_client = ui_dir / "api-client.ts"
    
    with open(api_client, 'r') as f:
        content = f.read()
    
    assert "getTransactions" in content or "get_transactions" in content, \
        "API client should have getTransactions method"


def test_api_client_has_create_transaction_method(ui_dir):
    """Test E3: API client has createTransaction method."""
    api_client = ui_dir / "api-client.ts"
    
    with open(api_client, 'r') as f:
        content = f.read()
    
    assert "createTransaction" in content or "create_transaction" in content, \
        "API client should have createTransaction method"


def test_api_client_has_get_balance_method(ui_dir):
    """Test E4: API client has getBalance method."""
    api_client = ui_dir / "api-client.ts"
    
    with open(api_client, 'r') as f:
        content = f.read()
    
    assert "getBalance" in content or "get_balance" in content, \
        "API client should have getBalance method"


def test_api_client_has_token_storage(ui_dir):
    """Test: API client stores authentication token."""
    api_client = ui_dir / "api-client.ts"
    
    with open(api_client, 'r') as f:
        content = f.read()
    
    assert "token" in content.lower(), \
        "API client should handle token storage"


def test_api_client_has_authorization_header(ui_dir):
    """Test: API client includes Authorization header."""
    api_client = ui_dir / "api-client.ts"
    
    with open(api_client, 'r') as f:
        content = f.read()
    
    assert "Authorization" in content or "authorization" in content, \
        "API client should include Authorization header"


def test_api_client_has_error_handling(ui_dir):
    """Test: API client has error handling."""
    api_client = ui_dir / "api-client.ts"
    
    with open(api_client, 'r') as f:
        content = f.read()
    
    assert "catch" in content or "error" in content.lower(), \
        "API client should have error handling"


def test_api_client_has_type_definitions(ui_dir):
    """Test: API client has TypeScript type definitions."""
    api_client = ui_dir / "api-client.ts"
    
    with open(api_client, 'r') as f:
        content = f.read()
    
    assert "interface" in content or "type " in content, \
        "API client should have TypeScript type definitions"


def test_types_file_exists(ui_dir):
    """Test: types.ts file exists for shared types."""
    types_file = ui_dir / "types.ts"
    assert types_file.exists(), "types.ts should exist for shared type definitions"


def test_types_has_transaction_type(ui_dir):
    """Test: types.ts has Transaction type."""
    types_file = ui_dir / "types.ts"
    
    with open(types_file, 'r') as f:
        content = f.read()
    
    assert "Transaction" in content, \
        "types.ts should define Transaction type"


def test_types_has_login_response_type(ui_dir):
    """Test: types.ts has LoginResponse type."""
    types_file = ui_dir / "types.ts"
    
    with open(types_file, 'r') as f:
        content = f.read()
    
    assert "LoginResponse" in content, \
        "types.ts should define LoginResponse type"


def test_types_has_balance_type(ui_dir):
    """Test: types.ts has Balance type."""
    types_file = ui_dir / "types.ts"
    
    with open(types_file, 'r') as f:
        content = f.read()
    
    assert "Balance" in content, \
        "types.ts should define Balance type"


def main():
    """Run all API client tests."""
    validator = ApiClientValidator()
    
    print("\nRunning API Client Tests (TypeScript)")
    print("="*60)
    print("\nPhase: RED - Verifying client tests fail before implementation\n")
    
    tests = [
        (test_api_client_file_exists, "api-client.ts file exists"),
        (test_api_client_has_login_method, "API client has login method"),
        (test_api_client_has_get_transactions_method, "API client has getTransactions method"),
        (test_api_client_has_create_transaction_method, "API client has createTransaction method"),
        (test_api_client_has_get_balance_method, "API client has getBalance method"),
        (test_api_client_has_token_storage, "API client stores token"),
        (test_api_client_has_authorization_header, "API client includes Authorization header"),
        (test_api_client_has_error_handling, "API client has error handling"),
        (test_api_client_has_type_definitions, "API client has type definitions"),
        (test_types_file_exists, "types.ts file exists"),
        (test_types_has_transaction_type, "types.ts has Transaction type"),
        (test_types_has_login_response_type, "types.ts has LoginResponse type"),
        (test_types_has_balance_type, "types.ts has Balance type"),
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
        print("\n✓ RED phase confirmed: API client tests are failing as expected.")
        print("Next step: Implement TypeScript API client to make tests pass (GREEN phase)")
        return 1
    else:
        print("\n✓ All API client tests passed!")
        return 0


if __name__ == "__main__":
    import sys
    sys.exit(main())
