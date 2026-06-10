"""
API contract validation tests.

Tests verify that all API endpoints are properly documented with request/response schemas.
"""

import json
from pathlib import Path


class ContractValidator:
    """Validates API contracts JSON file."""
    
    def __init__(self):
        self.contracts = self._load_contracts()
    
    def _load_contracts(self):
        """Load the contracts.json file."""
        contracts_path = Path(__file__).parent / "contracts.json"
        with open(contracts_path, 'r') as f:
            return json.load(f)
    
    def run_test(self, test_func, test_name):
        """Run a single test function."""
        try:
            test_func(self.contracts)
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


# Test functions for API contracts
def test_auth_login_endpoint_defined(contracts):
    """Test C1: POST /api/auth/login endpoint is defined."""
    assert 'endpoints' in contracts, "Contracts should have 'endpoints' section"
    
    endpoints = contracts['endpoints']
    auth_login = None
    
    for endpoint in endpoints:
        if endpoint.get('path') == '/api/auth/login' and endpoint.get('method') == 'POST':
            auth_login = endpoint
            break
    
    assert auth_login is not None, "POST /api/auth/login endpoint should be defined"


def test_auth_login_request_schema(contracts):
    """Test C1: Auth login has request schema."""
    endpoints = contracts.get('endpoints', [])
    auth_login = next((e for e in endpoints if e.get('path') == '/api/auth/login'), None)
    
    assert auth_login is not None, "Auth login endpoint should exist"
    assert 'request' in auth_login, "Auth login should have request schema"
    
    request_schema = auth_login['request']
    assert 'username' in request_schema.get('properties', {}), "Request should have username"
    assert 'password' in request_schema.get('properties', {}), "Request should have password"


def test_auth_login_response_schema(contracts):
    """Test C1: Auth login has response schema."""
    endpoints = contracts.get('endpoints', [])
    auth_login = next((e for e in endpoints if e.get('path') == '/api/auth/login'), None)
    
    assert auth_login is not None, "Auth login endpoint should exist"
    assert 'response' in auth_login, "Auth login should have response schema"
    
    response_schema = auth_login['response']
    assert 'token' in response_schema.get('properties', {}), "Response should have token"
    assert 'accountId' in response_schema.get('properties', {}), "Response should have accountId"
    assert 'accountName' in response_schema.get('properties', {}), "Response should have accountName"


def test_get_transactions_endpoint_defined(contracts):
    """Test C2: GET /api/transactions endpoint is defined."""
    endpoints = contracts.get('endpoints', [])
    get_transactions = next((e for e in endpoints 
                            if e.get('path') == '/api/transactions' 
                            and e.get('method') == 'GET'), None)
    
    assert get_transactions is not None, "GET /api/transactions endpoint should be defined"


def test_get_transactions_requires_auth(contracts):
    """Test C2: GET /api/transactions requires authentication."""
    endpoints = contracts.get('endpoints', [])
    get_transactions = next((e for e in endpoints if e.get('path') == '/api/transactions'), None)
    
    assert get_transactions is not None, "Transactions endpoint should exist"
    assert get_transactions.get('requiresAuth') is True, "Should require authentication"


def test_get_transactions_response_schema(contracts):
    """Test C2: GET /api/transactions has response schema."""
    endpoints = contracts.get('endpoints', [])
    get_transactions = next((e for e in endpoints if e.get('path') == '/api/transactions'), None)
    
    assert get_transactions is not None, "Transactions endpoint should exist"
    assert 'response' in get_transactions, "Should have response schema"
    
    response = get_transactions['response']
    assert response.get('type') == 'array', "Response should be array"
    assert 'items' in response, "Array should have items schema"
    
    item_schema = response['items'].get('properties', {})
    required_fields = ['id', 'sourceAccountId', 'targetAccountId', 'amount', 'date']
    for field in required_fields:
        assert field in item_schema, f"Transaction should have {field} field"


def test_create_transaction_endpoint_defined(contracts):
    """Test C3: POST /api/transactions endpoint is defined."""
    endpoints = contracts.get('endpoints', [])
    create_transaction = next((e for e in endpoints 
                              if e.get('path') == '/api/transactions' 
                              and e.get('method') == 'POST'), None)
    
    assert create_transaction is not None, "POST /api/transactions endpoint should be defined"


def test_create_transaction_request_schema(contracts):
    """Test C3: POST /api/transactions has request schema."""
    endpoints = contracts.get('endpoints', [])
    create_transaction = next((e for e in endpoints 
                              if e.get('path') == '/api/transactions' 
                              and e.get('method') == 'POST'), None)
    
    assert create_transaction is not None, "Create transaction endpoint should exist"
    assert 'request' in create_transaction, "Should have request schema"
    
    request_schema = create_transaction['request'].get('properties', {})
    assert 'targetAccountId' in request_schema, "Request should have targetAccountId"
    assert 'amount' in request_schema, "Request should have amount"
    assert 'date' in request_schema, "Request should have date"


def test_get_balance_endpoint_defined(contracts):
    """Test C4: GET /api/accounts/balance endpoint is defined."""
    endpoints = contracts.get('endpoints', [])
    get_balance = next((e for e in endpoints 
                       if e.get('path') == '/api/accounts/balance' 
                       and e.get('method') == 'GET'), None)
    
    assert get_balance is not None, "GET /api/accounts/balance endpoint should be defined"


def test_get_balance_response_schema(contracts):
    """Test C4: GET /api/accounts/balance has response schema."""
    endpoints = contracts.get('endpoints', [])
    get_balance = next((e for e in endpoints if e.get('path') == '/api/accounts/balance'), None)
    
    assert get_balance is not None, "Balance endpoint should exist"
    assert 'response' in get_balance, "Should have response schema"
    
    response_schema = get_balance['response'].get('properties', {})
    assert 'accountId' in response_schema, "Response should have accountId"
    assert 'accountName' in response_schema, "Response should have accountName"
    assert 'balance' in response_schema, "Response should have balance"


def main():
    """Run all API contract tests."""
    validator = ContractValidator()
    
    print("\nRunning API Contract Tests")
    print("="*60)
    print("\nPhase: RED - Verifying contract tests fail before implementation\n")
    
    tests = [
        (test_auth_login_endpoint_defined, "Auth login endpoint is defined"),
        (test_auth_login_request_schema, "Auth login has request schema"),
        (test_auth_login_response_schema, "Auth login has response schema"),
        (test_get_transactions_endpoint_defined, "Get transactions endpoint is defined"),
        (test_get_transactions_requires_auth, "Get transactions requires auth"),
        (test_get_transactions_response_schema, "Get transactions has response schema"),
        (test_create_transaction_endpoint_defined, "Create transaction endpoint is defined"),
        (test_create_transaction_request_schema, "Create transaction has request schema"),
        (test_get_balance_endpoint_defined, "Get balance endpoint is defined"),
        (test_get_balance_response_schema, "Get balance has response schema"),
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
        print("\n✓ RED phase confirmed: Contract tests are failing as expected.")
        print("Next step: Implement contracts.json to make tests pass (GREEN phase)")
        return 1
    else:
        print("\n✓ All API contract tests passed!")
        return 0


if __name__ == "__main__":
    import sys
    sys.exit(main())
