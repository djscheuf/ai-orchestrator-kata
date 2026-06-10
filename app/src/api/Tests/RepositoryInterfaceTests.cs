using System;
using System.Reflection;
using Xunit;
using FinancialApp.API.Repositories;

namespace FinancialApp.API.Tests;

public class RepositoryInterfaceTests
{
    [Fact]
    public void ITransactionRepository_HasGetByAccountIdAsyncMethod()
    {
        // Arrange
        var interfaceType = typeof(ITransactionRepository);
        
        // Act
        var method = interfaceType.GetMethod("GetByAccountIdAsync");
        
        // Assert
        Assert.NotNull(method);
        Assert.True(method.IsAsync() || method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>));
    }
    
    [Fact]
    public void ITransactionRepository_HasCreateAsyncMethod()
    {
        // Arrange
        var interfaceType = typeof(ITransactionRepository);
        
        // Act
        var method = interfaceType.GetMethod("CreateAsync");
        
        // Assert
        Assert.NotNull(method);
        Assert.True(method.IsAsync() || method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>));
    }
    
    [Fact]
    public void ITransactionRepository_HasGetAllAsyncMethod()
    {
        // Arrange
        var interfaceType = typeof(ITransactionRepository);
        
        // Act
        var method = interfaceType.GetMethod("GetAllAsync");
        
        // Assert
        Assert.NotNull(method);
        Assert.True(method.IsAsync() || method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>));
    }
    
    [Fact]
    public void IAccountRepository_HasGetByIdAsyncMethod()
    {
        // Arrange
        var interfaceType = typeof(IAccountRepository);
        
        // Act
        var method = interfaceType.GetMethod("GetByIdAsync");
        
        // Assert
        Assert.NotNull(method);
        Assert.True(method.IsAsync() || method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>));
    }
    
    [Fact]
    public void IAccountRepository_HasGetBalanceAsyncMethod()
    {
        // Arrange
        var interfaceType = typeof(IAccountRepository);
        
        // Act
        var method = interfaceType.GetMethod("GetBalanceAsync");
        
        // Assert
        Assert.NotNull(method);
        Assert.True(method.IsAsync() || method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>));
    }
    
    [Fact]
    public void IAccountRepository_HasGetAllAsyncMethod()
    {
        // Arrange
        var interfaceType = typeof(IAccountRepository);
        
        // Act
        var method = interfaceType.GetMethod("GetAllAsync");
        
        // Assert
        Assert.NotNull(method);
        Assert.True(method.IsAsync() || method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>));
    }
    
    [Fact]
    public void IAccountSecurityRepository_HasGetByUsernameAsyncMethod()
    {
        // Arrange
        var interfaceType = typeof(IAccountSecurityRepository);
        
        // Act
        var method = interfaceType.GetMethod("GetByUsernameAsync");
        
        // Assert
        Assert.NotNull(method);
        Assert.True(method.IsAsync() || method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>));
    }
    
    [Fact]
    public void IAccountSecurityRepository_HasGetByAccountIdAsyncMethod()
    {
        // Arrange
        var interfaceType = typeof(IAccountSecurityRepository);
        
        // Act
        var method = interfaceType.GetMethod("GetByAccountIdAsync");
        
        // Assert
        Assert.NotNull(method);
        Assert.True(method.IsAsync() || method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>));
    }
}

internal static class MethodExtensions
{
    public static bool IsAsync(this MethodInfo method)
    {
        return method.ReturnType == typeof(Task) ||
               (method.ReturnType.IsGenericType && 
                method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>));
    }
}
