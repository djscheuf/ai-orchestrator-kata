using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using FinancialApp.API.Services;

namespace FinancialApp.API.Middleware;

/// <summary>
/// Middleware for validating JWT tokens on protected endpoints.
/// </summary>
public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IJwtTokenService _jwtTokenService;
    
    public JwtMiddleware(RequestDelegate next, IJwtTokenService jwtTokenService)
    {
        _next = next;
        _jwtTokenService = jwtTokenService;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        var token = ExtractTokenFromHeader(context);
        
        if (!string.IsNullOrEmpty(token))
        {
            var principal = _jwtTokenService.ValidateToken(token);
            if (principal != null)
            {
                context.User = principal;
            }
        }
        
        await _next(context);
    }
    
    private string ExtractTokenFromHeader(HttpContext context)
    {
        var authHeader = context.Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authHeader))
        {
            return null;
        }
        
        const string bearerScheme = "Bearer ";
        if (authHeader.StartsWith(bearerScheme))
        {
            return authHeader.Substring(bearerScheme.Length).Trim();
        }
        
        return null;
    }
}
