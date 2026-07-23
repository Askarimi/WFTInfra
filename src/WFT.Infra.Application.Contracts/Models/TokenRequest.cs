using System.Collections.Generic;

namespace WFT.Infra.Application.Contracts.Models
{
    public record TokenRequest(
        string UserId,
        string? UserName,
        IEnumerable<string> Roles,
        IDictionary<string, string>? Claims = null
    );
}


