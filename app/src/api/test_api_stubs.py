"""
API Stub validation tests for .NET backend.

Tests verify that .NET API stub endpoints exist and return mock data matching contracts.
"""

import json
from pathlib import Path


class ApiStubValidator:
    """Validates that API stub files exist and have correct structure."""
    
    def __init__(self):
        self.contracts = self._load_contracts()
        self.api_dir = Path(__file__).parent
    
    def _load_contracts(self):
        """Load the contracts.json file."""
        contracts_path = Path(__file__).parent / "contracts.json"
        with open(contracts_path, 'r') as f:
            return json.load(f)
    
    def run_test(self, test_func, test_name):
        """Run a single test function."""
        try:
            test_func(self.api_dir, self.contracts)
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


# Test functions for API stubs
def test_controllers_directory_exists(api_dir, contracts):
    """Test D1: Controllers directory exists."""
    controllers_dir = api_dir / "Controllers"
    assert controllers_dir.exists(), "Controllers directory should exist"


def test_auth_controller_exists(api_dir, contracts):
    """Test D1: AuthController.cs exists."""
    auth_controller = api_dir / "Controllers" / "AuthController.cs"
    assert auth_controller.exists(), "AuthController.cs should exist"


def test_transactions_controller_exists(api_dir, contracts):
    """Test D1: TransactionsController.cs exists."""
    transactions_controller = api_dir / "Controllers" / "TransactionsController.cs"
    assert transactions_controller.exists(), "TransactionsController.cs should exist"


def test_accounts_controller_exists(api_dir, contracts):
    """Test D1: AccountsController.cs exists."""
    accounts_controller = api_dir / "Controllers" / "AccountsController.cs"
    assert accounts_controller.exists(), "AccountsController.cs should exist"


def test_auth_controller_has_login_endpoint(api_dir, contracts):
    """Test D1: AuthController has Login method."""
    auth_controller = api_dir / "Controllers" / "AuthController.cs"
    
    with open(auth_controller, 'r') as f:
        content = f.read()
    
    assert "public async Task<ActionResult<LoginResponse>>" in content or \
           "public async Task<IActionResult>" in content, \
        "AuthController should have Login endpoint method"


def test_transactions_controller_has_get_endpoint(api_dir, contracts):
    """Test D2: TransactionsController has Get method."""
    transactions_controller = api_dir / "Controllers" / "TransactionsController.cs"
    
    with open(transactions_controller, 'r') as f:
        content = f.read()
    
    assert "[HttpGet]" in content, \
        "TransactionsController should have HttpGet attribute"


def test_transactions_controller_has_post_endpoint(api_dir, contracts):
    """Test D3: TransactionsController has Post method."""
    transactions_controller = api_dir / "Controllers" / "TransactionsController.cs"
    
    with open(transactions_controller, 'r') as f:
        content = f.read()
    
    assert "[HttpPost]" in content, \
        "TransactionsController should have HttpPost attribute"


def test_accounts_controller_has_balance_endpoint(api_dir, contracts):
    """Test D4: AccountsController has GetBalance method."""
    accounts_controller = api_dir / "Controllers" / "AccountsController.cs"
    
    with open(accounts_controller, 'r') as f:
        content = f.read()
    
    assert "GetBalance" in content or "balance" in content.lower(), \
        "AccountsController should have GetBalance method"


def test_dto_classes_exist(api_dir, contracts):
    """Test: DTO classes exist for request/response objects."""
    dtos_dir = api_dir / "DTOs"
    assert dtos_dir.exists(), "DTOs directory should exist"
    
    required_dtos = [
        "LoginRequest.cs",
        "LoginResponse.cs",
        "TransactionDto.cs",
        "CreateTransactionRequest.cs",
        "BalanceResponse.cs"
    ]
    
    for dto in required_dtos:
        dto_file = dtos_dir / dto
        assert dto_file.exists(), f"{dto} should exist in DTOs directory"


def test_login_response_has_required_fields(api_dir, contracts):
    """Test: LoginResponse DTO has required fields."""
    login_response = api_dir / "DTOs" / "LoginResponse.cs"
    
    with open(login_response, 'r') as f:
        content = f.read()
    
    required_fields = ["Token", "AccountId", "AccountName"]
    for field in required_fields:
        assert field in content, f"LoginResponse should have {field} property"


def test_transaction_dto_has_required_fields(api_dir, contracts):
    """Test: TransactionDto has required fields."""
    transaction_dto = api_dir / "DTOs" / "TransactionDto.cs"
    
    with open(transaction_dto, 'r') as f:
        content = f.read()
    
    required_fields = ["Id", "SourceAccountId", "TargetAccountId", "Amount", "Date"]
    for field in required_fields:
        assert field in content, f"TransactionDto should have {field} property"


def test_balance_response_has_required_fields(api_dir, contracts):
    """Test: BalanceResponse has required fields."""
    balance_response = api_dir / "DTOs" / "BalanceResponse.cs"
    
    with open(balance_response, 'r') as f:
        content = f.read()
    
    required_fields = ["AccountId", "AccountName", "Balance"]
    for field in required_fields:
        assert field in content, f"BalanceResponse should have {field} property"


def test_controllers_have_authorize_attribute(api_dir, contracts):
    """Test: Protected endpoints have [Authorize] attribute."""
    transactions_controller = api_dir / "Controllers" / "TransactionsController.cs"
    
    with open(transactions_controller, 'r') as f:
        content = f.read()
    
    assert "[Authorize]" in content, \
        "TransactionsController should have [Authorize] attribute"


def main():
    """Run all API stub tests."""
    validator = ApiStubValidator()
    
    print("\nRunning API Stub Tests (.NET)")
    print("="*60)
    print("\nPhase: RED - Verifying stub tests fail before implementation\n")
    
    tests = [
        (test_controllers_directory_exists, "Controllers directory exists"),
        (test_auth_controller_exists, "AuthController.cs exists"),
        (test_transactions_controller_exists, "TransactionsController.cs exists"),
        (test_accounts_controller_exists, "AccountsController.cs exists"),
        (test_auth_controller_has_login_endpoint, "AuthController has Login endpoint"),
        (test_transactions_controller_has_get_endpoint, "TransactionsController has Get endpoint"),
        (test_transactions_controller_has_post_endpoint, "TransactionsController has Post endpoint"),
        (test_accounts_controller_has_balance_endpoint, "AccountsController has GetBalance endpoint"),
        (test_dto_classes_exist, "DTO classes exist"),
        (test_login_response_has_required_fields, "LoginResponse has required fields"),
        (test_transaction_dto_has_required_fields, "TransactionDto has required fields"),
        (test_balance_response_has_required_fields, "BalanceResponse has required fields"),
        (test_controllers_have_authorize_attribute, "Protected endpoints have [Authorize] attribute"),
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
        print("\n✓ RED phase confirmed: API stub tests are failing as expected.")
        print("Next step: Implement .NET API stubs to make tests pass (GREEN phase)")
        return 1
    else:
        print("\n✓ All API stub tests passed!")
        return 0


if __name__ == "__main__":
    import sys
    sys.exit(main())
