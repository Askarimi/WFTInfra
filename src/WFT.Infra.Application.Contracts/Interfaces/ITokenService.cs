using System;
using System.Security.Claims;
using WFT.Infra.Application.Contracts.DTOs;
using WFT.Infra.Application.Contracts.DTOs.UserManagment;
using WFT.Infra.Application.Contracts.Models;

namespace WFT.Infra.Application.Contracts.Interfaces
{
    public partial interface ITokenService
    {
        // Clean, distinct contracts
        string GenerateToken(TokenRequest request);
        ClaimsPrincipal? ValidateToken(string token);
        Task<JwtResultDto?> RefreshTokenAsync(string refreshToken);
        IDictionary<string, string> ExtractClaims(string token);
    }
}
