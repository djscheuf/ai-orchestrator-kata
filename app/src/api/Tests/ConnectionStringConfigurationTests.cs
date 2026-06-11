using System;
using System.IO;
using Xunit;
using Microsoft.Extensions.Configuration;

namespace FinancialApp.API.Tests;

public class ConnectionStringConfigurationTests
{
    [Fact]
    public void Appsettings_ContainsConnectionString()
    {
        // Arrange
        var projectPath = Directory.GetCurrentDirectory();
        var appsettingsPath = Path.Combine(projectPath, "appsettings.json");

        // Act
        var config = new ConfigurationBuilder()
            .AddJsonFile(appsettingsPath, optional: false, reloadOnChange: false)
            .Build();

        var connectionString = config.GetConnectionString("DefaultConnection");

        // Assert
        Assert.NotNull(connectionString);
        Assert.NotEmpty(connectionString);
    }

    [Fact]
    public void AppsettingsDevelopment_ContainsConnectionString()
    {
        // Arrange
        var projectPath = Directory.GetCurrentDirectory();
        var appsettingsDevelopmentPath = Path.Combine(projectPath, "appsettings.Development.json");

        // Act
        var config = new ConfigurationBuilder()
            .AddJsonFile(appsettingsDevelopmentPath, optional: true, reloadOnChange: false)
            .Build();

        var connectionString = config.GetConnectionString("DefaultConnection");

        // Assert - Development settings are optional, but if present should have connection string
        if (File.Exists(appsettingsDevelopmentPath))
        {
            Assert.NotNull(connectionString);
            Assert.NotEmpty(connectionString);
        }
    }

    [Fact]
    public void ConnectionString_IsValidSqlServerFormat()
    {
        // Arrange
        var projectPath = Directory.GetCurrentDirectory();
        var appsettingsPath = Path.Combine(projectPath, "appsettings.json");

        var config = new ConfigurationBuilder()
            .AddJsonFile(appsettingsPath, optional: false, reloadOnChange: false)
            .Build();

        var connectionString = config.GetConnectionString("DefaultConnection");

        // Act & Assert
        Assert.NotNull(connectionString);
        // Verify it contains SQL Server connection string components
        Assert.True(connectionString.Contains("Server=") || connectionString.Contains("server="),
            "Connection string should contain Server parameter");
        Assert.True(connectionString.Contains("Database=") || connectionString.Contains("database="),
            "Connection string should contain Database parameter");
    }
}
