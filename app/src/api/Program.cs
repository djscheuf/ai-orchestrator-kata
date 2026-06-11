using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FinancialApp.API.Data;
using FinancialApp.API.Services;
using FinancialApp.API.Repositories;
using FinancialApp.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configure database connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(connectionString))
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString));
}

// Configure JWT authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "your-256-bit-secret-key-minimum-32-chars-long";
var key = Encoding.ASCII.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Register application services
builder.Services.AddSingleton<IJwtTokenService>(sp => new JwtTokenService(secretKey));
builder.Services.AddScoped<IAccountSecurityService, AccountSecurityService>();
builder.Services.AddScoped<ITransactionValidationService, TransactionValidationService>();
builder.Services.AddScoped<IBalanceCalculationService, BalanceCalculationService>();
builder.Services.AddScoped<ITransactionRepository, InMemoryTransactionRepository>();
builder.Services.AddScoped<IAccountRepository, InMemoryAccountRepository>();

// Add CORS if needed
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Seed in-memory repositories with test data
using (var scope = app.Services.CreateScope())
{
    var accountRepository = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
    if (accountRepository is InMemoryAccountRepository inMemoryAccountRepo)
    {
        inMemoryAccountRepo.AddAccount(new Account 
        { 
            Id = "550e8400-e29b-41d4-a716-446655440001", 
            AccountName = "Alice Account",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        inMemoryAccountRepo.AddAccount(new Account 
        { 
            Id = "550e8400-e29b-41d4-a716-446655440002", 
            AccountName = "Bob Account",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        inMemoryAccountRepo.AddAccount(new Account 
        { 
            Id = "550e8400-e29b-41d4-a716-446655440003", 
            AccountName = "Charlie Account",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        inMemoryAccountRepo.AddAccount(new Account 
        { 
            Id = "550e8400-e29b-41d4-a716-446655440004", 
            AccountName = "Diana Account",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        inMemoryAccountRepo.AddAccount(new Account 
        { 
            Id = "550e8400-e29b-41d4-a716-446655440005", 
            AccountName = "Eve Account",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<JwtMiddleware>();

app.MapControllers();

app.Run();
