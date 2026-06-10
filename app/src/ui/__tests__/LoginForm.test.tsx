import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { LoginForm } from '../components/LoginForm';

describe('LoginForm', () => {
  const mockOnLogin = jest.fn();

  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('should render username input', () => {
    render(<LoginForm onLogin={mockOnLogin} />);
    expect(screen.getByLabelText(/username/i)).toBeInTheDocument();
  });

  it('should render password input', () => {
    render(<LoginForm onLogin={mockOnLogin} />);
    expect(screen.getByLabelText(/password/i)).toBeInTheDocument();
  });

  it('should render submit button', () => {
    render(<LoginForm onLogin={mockOnLogin} />);
    expect(screen.getByRole('button', { name: /login/i })).toBeInTheDocument();
  });

  it('should call onLogin with credentials on submit', async () => {
    const user = userEvent.setup();
    render(<LoginForm onLogin={mockOnLogin} />);

    const usernameInput = screen.getByLabelText(/username/i);
    const passwordInput = screen.getByLabelText(/password/i);
    const submitButton = screen.getByRole('button', { name: /login/i });

    await user.type(usernameInput, 'testuser');
    await user.type(passwordInput, 'password123');
    await user.click(submitButton);

    await waitFor(() => {
      expect(mockOnLogin).toHaveBeenCalledWith({
        username: 'testuser',
        password: 'password123',
      });
    });
  });

  it('should display error message on login failure', async () => {
    const user = userEvent.setup();
    mockOnLogin.mockRejectedValue(new Error('Invalid credentials'));

    render(<LoginForm onLogin={mockOnLogin} />);

    const usernameInput = screen.getByLabelText(/username/i);
    const passwordInput = screen.getByLabelText(/password/i);
    const submitButton = screen.getByRole('button', { name: /login/i });

    await user.type(usernameInput, 'testuser');
    await user.type(passwordInput, 'wrongpassword');
    await user.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText(/invalid credentials/i)).toBeInTheDocument();
    });
  });

  it('should clear error on new login attempt', async () => {
    const user = userEvent.setup();
    mockOnLogin.mockRejectedValueOnce(new Error('Invalid credentials'));
    mockOnLogin.mockResolvedValueOnce(undefined);

    render(<LoginForm onLogin={mockOnLogin} />);

    const usernameInput = screen.getByLabelText(/username/i);
    const passwordInput = screen.getByLabelText(/password/i);
    const submitButton = screen.getByRole('button', { name: /login/i });

    await user.type(usernameInput, 'testuser');
    await user.type(passwordInput, 'wrongpassword');
    await user.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText(/invalid credentials/i)).toBeInTheDocument();
    });

    await user.clear(usernameInput);
    await user.clear(passwordInput);
    await user.type(usernameInput, 'testuser');
    await user.type(passwordInput, 'correctpassword');
    await user.click(submitButton);

    await waitFor(() => {
      expect(screen.queryByText(/invalid credentials/i)).not.toBeInTheDocument();
    });
  });

  it('should show loading state during login', async () => {
    const user = userEvent.setup();
    mockOnLogin.mockImplementation(
      () =>
        new Promise((resolve) => setTimeout(resolve, 100))
    );

    render(<LoginForm onLogin={mockOnLogin} />);

    const usernameInput = screen.getByLabelText(/username/i);
    const passwordInput = screen.getByLabelText(/password/i);
    const submitButton = screen.getByRole('button', { name: /login/i });

    await user.type(usernameInput, 'testuser');
    await user.type(passwordInput, 'password123');
    await user.click(submitButton);

    expect(screen.getByRole('button', { name: /loading/i })).toBeInTheDocument();
  });

  it('should disable inputs during login', async () => {
    const user = userEvent.setup();
    mockOnLogin.mockImplementation(
      () =>
        new Promise((resolve) => setTimeout(resolve, 100))
    );

    render(<LoginForm onLogin={mockOnLogin} />);

    const usernameInput = screen.getByLabelText(/username/i) as HTMLInputElement;
    const passwordInput = screen.getByLabelText(/password/i) as HTMLInputElement;
    const submitButton = screen.getByRole('button', { name: /login/i });

    await user.type(usernameInput, 'testuser');
    await user.type(passwordInput, 'password123');
    await user.click(submitButton);

    expect(usernameInput.disabled).toBe(true);
    expect(passwordInput.disabled).toBe(true);
  });

  it('should validate required fields', async () => {
    const user = userEvent.setup();
    render(<LoginForm onLogin={mockOnLogin} />);

    const submitButton = screen.getByRole('button', { name: /login/i });
    await user.click(submitButton);

    expect(mockOnLogin).not.toHaveBeenCalled();
  });
});
